from flask import Flask, request, jsonify
from flask_restful import Api, Resource #, reqparse
from flask_httpauth import HTTPBasicAuth
from flask_apispec import marshal_with, doc, use_kwargs
from flask_apispec.views import MethodResource
from flask_apispec.extension import FlaskApiSpec

from apispec import APISpec
from apispec.ext.marshmallow import MarshmallowPlugin
from marshmallow import Schema, fields, ValidationError

from arxiv_public_data.fulltext import fulltext as pdf_to_str
from passlib.apps import custom_app_context as pwd_context

import requests
import pickle, joblib
import tempfile
import logging
import json
import os
import pdb

import text_classification as texcls
from keyphrase_extraction import KeyphraseExtractor

logger = logging.getLogger(__name__)
logger.setLevel(logging.ERROR)

app = Flask(__name__)
app.logger.setLevel(logging.ERROR)
api = Api(app)
app.config.update({
    'APISPEC_SPEC': APISpec(
        title='Hercules REST API for descriptors',
        version='1.0',
        openapi_version='3.0.0',
        info=dict(
            description='This module classifies papers, protocols or open-source code projects accoding to a predefined thematic descriptor taxonomy and free specific descriptors.'
        ),
        plugins=[MarshmallowPlugin()]
    ),
    'APISPEC_SWAGGER_URL': '/doc-json/',  # URI to access API Doc JSON 
    'APISPEC_SWAGGER_UI_URL': '/doc/'  # URI to access UI of API Doc
})
docs = FlaskApiSpec(app)

auth = HTTPBasicAuth()


# Api documentation

class ThematicRequestSchema(Schema):
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions | 'papers'=scientific papers.")
    text = fields.String(required=False, description="Input text to classify")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=False, description="Description/abstract of the Input article to classify")
    journal = fields.String(required=False, description="Journal where the paper was published")
    author_name = fields.String(required=False, description="Author(s) of the paper")
    author_affiliation = fields.String(required=False, description="Author(s)'s affilliation(s)")
    pdf_url = fields.String(required=False, description="URL of the pdf version of the paper. If pdf_url is given no other parameter is required, except for the rotype")
        
class SpecificRequestSchema(Schema):
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions.")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=True, description="Abstract of the Input article to classify")
    body = fields.String(required=False, description="Main text of the paper. If no body is present the systems resorts to short texts keyphrase extractor")

class ThematicResponseSchema(Schema):
    text = fields.String(required=False, description="Input text to classify")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=False, description="Description/abstract of the Input article to classify")
    journal = fields.String(required=False, description="Journal where the paper was published")
    author_name = fields.String(required=False, description="Author(s) of the paper")
    author_affiliation = fields.String(required=False, description="Author(s)'s affilliation(s)")
    pdf_url = fields.String(required=False, description="URL of the pdf version of the paper. If pdf_url is given no other parameter is required, except for the rotype")
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions.")
    topics = fields.Dict(keys=fields.Str(), values=fields.Str())

class SpecificResponseSchema(Schema):
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions.")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=True, description="Abstract of the Input article to classify")
    body = fields.String(required=False, description="Main text of the paper. If no body is present the systems resorts to short texts keyphrase extractor")
    topics = fields.Dict(keys=fields.Str(), values=fields.Str())

    
conf = {}
topic_models = {}
keyphrase_extractors = {}
available_topic_models = ['sourceForge', 'bio-protocol', 'papers']
available_keyphrase_models = ['papers']


################################################################

##    Helper function                                         ##

################################################################

"""
Raises ValueError if URL is not valid or it can't be accessed
Returns empty string if an error occurs while extracting text from the PDF
"""
def pdf_to_text(url):

    
    headers = {
        'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.76 Safari/537.36'
    }

    r = requests.get(url, headers=headers)

    if len(str(r.status_code)) == 0 or str(r.status_code)[0] != '2':
        raise ValueError("Invalid URL")
    
    with tempfile.NamedTemporaryFile(delete=False) as tmp:
        tmp.write(r.content)
        print(tmp.name)
        try:
            text = pdf_to_str(tmp.name)
        except Exception as e:
            logging.error("Could not convert pdf to text: {} ".format(str(e)))
            text = ""

    return text


################################################################

#  API related functions

################################################################
 
def init():
    global conf
    global topic_models
    global keyphrase_extractors
    
    # load configuration
    CONFDIR = os.path.dirname(os.path.realpath(__file__))
    with open('{}/conf.json'.format(CONFDIR), 'r', encoding='utf-8') as f:
        conf = json.load(f)

    if not 'topics' in conf: 
        return make_error(500, "topic taxonomy not present in config. This is a server side problem contact with the API maintainers")
            
    #load models in memory for topic classification
    for dimension in conf['topics']:
        logging.info("dimension: {}".format(dimension))
        model_name=dimension["name"]
        model_path=dimension["path"]        
        topic_models[model_name] = texcls.TrTextClassifier(model_path)
        logging.info("model {} loaded".format(model_name))
    
    logging.info("{} topic models loaded".format(len(topic_models)))

    #load models for keyphrase extraction
    if not 'keyphrases' in conf: 
        return make_error(500, "keyphrases model information not present in config. This is a server side problem contact with the API maintainers")

    for dimension in conf['keyphrases']:
        logging.info("dimension: {}".format(dimension))
        model_name=dimension["name"]
        model_path=dimension["path"]

        if model_name not in available_keyphrase_models:
             logging.warning("model {} not available".format(model_name))
             continue

        # loading models
        print("Loading freqs and idf models")
        kp_clef_en_fpath=os.path.join(model_path,"clef_en.pkl")
        kp_clef_es_fpath=os.path.join(model_path,"clef_es.pkl")
        kp_clef_idf_en_fpath=os.path.join(model_path,"idfakCLEF_en.pkl")
        kp_clef_idf_es_fpath=os.path.join(model_path,"idfakCLEF_es.pkl")
        kp_scopus_fpath=os.path.join(model_path,"scopus.pkl")
        kp_clef, kp_clef_idf = {}, {}
        with open(kp_clef_en_fpath, 'rb') as f:
            kp_clef['en'] = pickle.load(f)
        with open(kp_clef_es_fpath, 'rb') as f:
            kp_clef['es'] = pickle.load(f)
        with open(kp_scopus_fpath, 'rb') as f:
            kp_scopus = pickle.load(f)
        kp_clef_idf['en'] = joblib.load(kp_clef_idf_en_fpath)
        kp_clef_idf['es'] = joblib.load(kp_clef_idf_es_fpath)
        print("Done")
        
        for i in ['short','fulltext']:
            kp_model_s_fpath=os.path.join(model_path,"single-"+i+".sav")
            kp_model_m_fpath=os.path.join(model_path,"multi-"+i+".sav")
            kp_model_s = joblib.load(kp_model_s_fpath)
            kp_model_m = joblib.load(kp_model_m_fpath)
            
            model=KeyphraseExtractor(kp_model_s, kp_model_m, kp_clef, kp_scopus, kp_clef_idf) #kp_scopus_fpath)

            if model == None:
                logging.warning("model {} could not be loaded".format(model_name))
                continue
            
            if not model_name in keyphrase_extractors:
                keyphrase_extractors[model_name]={}

            keyphrase_extractors[model_name][i]=model

    logging.info("{} Keyphrase extraction models loaded".format(len(keyphrase_extractors)))


if not app.debug or os.environ.get("WERKZEUG_RUN_MAIN") == "true":
    init()

@auth.verify_password
def verify_password(username, password):
    user = None
    pass_crypt=pwd_context.encrypt(password)
    for _user in conf['users']:
        if _user['username'] == username:
            user = _user
    if user and pwd_context.verify(password, user['password']):
        logging.info("correct password")
        return True
    logging.error("incorrect password - {} - {} -".format(username,password))
    return False

def make_error(code, msg):
    logging.error('Error {}: {}'.format(code, msg))
    response = jsonify({
        'error': msg,
    })
    response.status_code = code
    return response


class ThematicDescriptorAPI(MethodResource, Resource):
    #decorators = [auth.login_required]
    @doc(description='Hercules thematic keyword API.', tags=['Hercules'])
    @use_kwargs(ThematicRequestSchema, location='json')
    @marshal_with(ThematicResponseSchema, description="returns the request object + a new property called topics, which contains a dictionary with the tags found and theis probabilities")  # marshalling with marshmallow
    def post(self, **kwargs):
        """post represent the a POST API method.

        returns the tags found and their probabilities.
        ---
        post:
              parameters:
        - in: path
             schema: ThematicRequestSchema
      responses:
        200:
          content:
            application/json:
              schema: ThematicResponseSchema
        """
        logging.debug("kwargs: {} \n \n".format(kwargs))
        json_data=request.get_json(force=True)
        response_json = json_data
        schema = ThematicRequestSchema()
        try:
            # Validate request body against schema data types
            result = schema.load(json_data)
            if 'pdf_url' not in json_data and 'title' not in json_data and 'abstract' not in json_data:
                return jsonify("error: either 'pdf_url', or 'title' and 'abstract' fields are required"), 400
        except ValidationError as err:
            logging.error(err.messages, err,json_data)
            return jsonify(err.messages), 400
        
        logging.info(json.dumps(json_data, indent=4, sort_keys=True))        
        
        text=""
        if 'pdf_url' in json_data:
            pdf_url_path=json_data['pdf_url']
            try:
                text=pdf_to_text(pdf_url_path)
            except Exception as e:
                return make_error(501, "'pdf_url' specified, but no text could be extracted from the given url - " + str(e))
            
        if text == "":
            if 'journal' in json_data:
                text=json_data['journal']
            if 'author_name' in json_data:
                text=text+" \n "+json_data['author_name']
            if 'author_affiliation' in json_data:
                text=text+" \n "+json_data['author_affiliation']
            if 'title' in json_data:
                text=text+" \n "+json_data['title']    
            if  'abstract' in json_data:
                text=text+" \n "+json_data['abstract']

        if json_data['rotype'] not in available_topic_models:
            response_json["topics"]={'error':'specified ro type is not available {}'.format(available_topic_models)}
            return response_json
        
        text=text.replace("<br />","\n")

        if text == "": ## text is empty, no classification will be done.
            return make_error(400, "Neither could text be extracted from pdf, nor the other fields could be used, there is nothing to tag.")
        else: ## start clasifications
            ## topic classification
            topics = []
            try:                
                topics = self.classify_thematic_descriptors(text.replace("\n","").replace("  "," "),json_data['rotype'])
                response_json["topics"]=topics
            except Exception as e:
                return make_error(501, "Error when computing topic classification: " + str(e))

        return jsonify(response_json)

    def classify_thematic_descriptors(self,text,topic_model=None):
        results={}
        logging.info(f"Topic model: {topic_model}")
        logging.info(f"All loaded topic models: {list(topic_models.keys())}")
        if topic_model != None and topic_model in topic_models:
            logging.info("************ Tagging with {} model ************* \n".format(topic_model))
            model = topic_models[topic_model]
            result = model.classify(text)[0]
            one_pred = [ res[0] for res in result ]
            one_prob = [ res[1] for res in result ]
            
            logging.info("************* Prediction ready: \n Predictions: {} \n probabilities: {}".format(one_pred, one_prob))
            
            results=[ {"word":one_pred[i], "porcentaje": str(f"{one_prob[i]:.2f}")} for i in range(len(one_pred)) if (one_prob[i] > 0.5)]
            logging.info("************* Final Result: {} \n".format(results))
            
        return results

    
class SpecificDescriptorAPI(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Hercules Specific keyword API endpoint.', tags=['Hercules, Specific keywords'])
    @use_kwargs(SpecificRequestSchema, location='json')
    @marshal_with(SpecificResponseSchema, description="returns the request object + a new property called topics, which contains a dictionary with the tags found and theis probabilities")
    def post(self, **kwargs):
        """post represent the a POST API method.

        returns the tags found and their probabilities.
        ---
        post:
              parameters:
        - in: path
             schema: SpecificRequestSchema
      responses:
        200:
          content:
            application/json:
              schema: SpecificResponseSchema
        """
        logging.debug("kwargs: {} \n \n".format(kwargs))
        json_data=request.get_json(force=True)
        response_json = json_data
        schema = SpecificRequestSchema()
        try:
            # Validate request body against schema data types
            result = schema.load(json_data)
        except ValidationError as err:
            logging.error(err.messages, err,json_data)
            return jsonify(err.messages), 400
        
        logging.info(json.dumps(json_data, indent=4, sort_keys=True))

        text=""
        title=""
        abstract=""
        body=""
        
        if 'journal' in json_data:
            text=json_data['journal']
        if 'author_name' in json_data:
            text=text+" \n "+json_data['author_name']
        if 'author_affiliation' in json_data:
            text=text+" \n "+json_data['author_affiliation']
        if 'title' in json_data:
            title=json_data['title']
            text=title+" \n "+json_data['title']
        if  'abstract' in json_data:
            abstract=json_data['abstract']
            text=text+" \n "+abstract
        if  'body' in json_data:
            body=json_data['body']
            text=text+" \n "+body

        #if text == "" and 'pdf_url' in json_data:
        #    pdf_url_path=json_data['pdf_url']
        #    try:
        #        text=pdf_to_text(pdf_url_path)
        #    except:
        #        return jsonify("error: 'pdf_url' specified, but no text could be extracted from the given url"), 400
        #    logging.error('Warn: text element extracted from pdf url: _{}_  \n'.format(text))
    
        rotype=json_data['rotype']
        
        if rotype not in available_keyphrase_models:
            response_json["topics"]={'error':'specified ro type is not available {}'.format(available_keyphrase_models)}
            return response_json
        
        logging.info('Text to process: \n TITLE:_{}_ \n ABSTRACT:_{}_ \n TEXT:_{}_'.format(title, abstract, text))
        text=text.replace("<br />","\n")
        title=title.replace("<br />","\n")
        abstract=abstract.replace("<br />","\n")
        body=body.replace("<br />","\n")
        
        if text == "" or (title == "" and abstract == "" and body == ""): ## text is empty, no classification will be done.
            logging.error("Text is empty, no classification will be done")
            return make_error(400, "'text', or ('title', 'abstract' and 'body') elements are null or missing, there is nothing to tag.")
       
        else: ## start keyphrase extraction
            keyphrases = []            
            try:
                if rotype not in keyphrase_extractors:
                    return make_error(501, "No extractor found for the give ro type: {}".format(rotype))
                text_length="fulltext"
                if body == "": #body is empty, go for short text extractor
                    text_length="short"

                keyphrases = keyphrase_extractors[rotype][text_length].extract_keyphrases(title, abstract, body)
                logging.info('keyphrases: {}'.format(keyphrases))
                term_list=[]
                for i,k in enumerate(keyphrases):
                    term_list.append({"word":k,"porcentaje":str(f"{keyphrases[k]:.4f}")})
                response_json["topics"]=term_list
                
            except Exception as e:
                return make_error(501, "Error when extracting keyphrases: " + str(e))

        return jsonify(response_json)

        
api.add_resource(ThematicDescriptorAPI, '/thematic')
docs.register(ThematicDescriptorAPI)

api.add_resource(SpecificDescriptorAPI, '/specific')
docs.register(SpecificDescriptorAPI)


# This error handler is necessary for usage with Flask-RESTful
'''
@parser.error_handler
def handle_request_parsing_error(err, req, schema, *, error_status_code, error_headers):
    """webargs error handler that uses Flask-RESTful's abort function to return
    a JSON error response to the client.
    """
    print("****** Request: {} \n schema {} \n ******\n".format(req,schema))
    abort(error_status_code, err.messages)
'''

if __name__ == "__main__":
    app.run()
