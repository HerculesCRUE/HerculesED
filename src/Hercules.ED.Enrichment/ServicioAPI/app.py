from flask import Flask, request, jsonify
from flask_restful import Api, Resource #, reqparse
from flask_httpauth import HTTPBasicAuth

from flask_apispec import marshal_with, doc, use_kwargs
from flask_apispec.views import MethodResource
from marshmallow import Schema, fields, ValidationError

from apispec import APISpec
from apispec.ext.marshmallow import MarshmallowPlugin
from flask_apispec.extension import FlaskApiSpec

from webargs import fields, validate
from webargs.flaskparser import use_args, parser, abort

from passlib.apps import custom_app_context as pwd_context
import datetime
import socket
from socket import error as socket_error
import json
import sys
import os
import shutil
import logging
from logging.handlers import RotatingFileHandler
import translitcodec
import codecs

import joblib
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from cmath import polar
import subprocess

from nltk.tokenize import RegexpTokenizer
import hunspell


# for pdf to text conversion
import requests
import tempfile
from arxiv_public_data.fulltext import fulltext as pdf_to_str

## from here on import required for neural models
import re

from neural_multilabel_classification import set_seed,get_labels,convert_examples_to_features_multi,evaluate,load_neural_model,init_neural_resources
from keyphrase_extraction import KeyphraseExtractor


logger = logging.getLogger(__name__)
logger.setLevel(logging.ERROR)

## Global variables 
app = Flask(__name__)
app.logger.setLevel(logging.ERROR)
api = Api(app)
app.config.update({
    'APISPEC_SPEC': APISpec(
        title='Hercules RESTplus API for subject keywords Demo',
        version='1.0',
        openapi_version='3.0.0',
        info=dict(
            description='Application to classify protocol or github code texts accoding to a defined keyword taxonomy.'
        ),
        plugins=[MarshmallowPlugin()]
    ),
    'APISPEC_SWAGGER_URL': '/doc-json/',  # URI to access API Doc JSON 
    'APISPEC_SWAGGER_UI_URL': '/doc/'  # URI to access UI of API Doc
})
docs = FlaskApiSpec(app)



auth = HTTPBasicAuth()

## Api documentation

class HerculesRequestSchema(Schema):
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions.")
    text = fields.String(required=False, description="Input text to classify")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=False, description="Description/abstract of the Input article to classify")
    journal = fields.String(required=False, description="Journal where the paper was published")
    author_name = fields.String(required=False, description="Author(s) of the paper")
    author_affiliation = fields.String(required=False, description="Author(s)'s affilliation(s)")
    pdf_url = fields.String(required=False, description="URL of the pdf version of the paper. If pdf_url is given no other parameter is required, except for the rotype")
    
    
class HerculesKeyphraseRequestSchema(Schema):
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions.")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=True, description="Abstract of the Input article to classify")
    body = fields.String(required=False, description="Main text of the paper. If no body is present the systems resorts to short texts keyphrase extractor")

class HerculesResponseSchema(Schema):
    text = fields.String(required=False, description="Input text to classify")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=False, description="Description/abstract of the Input article to classify")
    journal = fields.String(required=False, description="Journal where the paper was published")
    author_name = fields.String(required=False, description="Author(s) of the paper")
    author_affiliation = fields.String(required=False, description="Author(s)'s affilliation(s)")
    pdf_url = fields.String(required=False, description="URL of the pdf version of the paper. If pdf_url is given no other parameter is required, except for the rotype")
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions.")
    topics = fields.Dict(keys=fields.Str(), values=fields.Str())


class HerculesKeyphraseResponseSchema(Schema):
    rotype = fields.String(required=True, description="Type of the input text, currently available: 'bio-protocol'=protocol descriptions | 'sourceForge'=software descriptions.")
    title = fields.String(required=False, description="Title of the input article to classify")
    abstract = fields.String(required=True, description="Abstract of the Input article to classify")
    body = fields.String(required=False, description="Main text of the paper. If no body is present the systems resorts to short texts keyphrase extractor")
    topics = fields.Dict(keys=fields.Str(), values=fields.Str())

    
conf = {}
taxonomy = {}
topic_models = {}
available_topic_models = ['sourceForge','bio-protocol','papers']
tokenizer = "" 
tmp_path = "/tmp"
keyphrase_extractors={}
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
    global available_topic_models
    global topic_models
    global tokenizer
    global tmp_path
    global keyphrase_extractors
    global available_keyphrase_models
    
    # load configuration
    CONFDIR = os.path.dirname(os.path.realpath(__file__))
    with open('{}/conf.json'.format(CONFDIR), 'r', encoding='utf-8') as f:
        conf = json.load(f)

    if "tmp_path" in conf:        
        tmp_path=conf["tmp_path"]
    else:
        print("WARN no tmp directory specified, defaulting to /tmp/gnoss-hercules-api")
        tmp_path="/tmp/gnoss-hercules-api"
        
    os.makedirs(tmp_path,exist_ok=True)


    if not 'topics' in conf: 
        return make_error(500, "topic taxonomy not present in config. This is a server side problem contact with the API maintainers")

    init_neural_resources(conf["device"])
    
        
    #load models in memory for topic classification
    for dimension in conf['topics']:
        print ("dimension: {}".format(dimension))
        model_name=dimension["name"]
        model_path=dimension["path"]
        model_type=dimension["type"]
        
        (tokenizer,model,label2id,id2label,pad_token_label_id)=load_neural_model(model_name,model_path,tmp_path)
        if model == None:
            print ("model {} could not be loaded".format(model_name))
            continue
            
        if not model_name in topic_models:
            topic_models[model_name]={}
            
        topic_models[model_name]["tokenizer"]=tokenizer
        topic_models[model_name]["model"]=model
        topic_models[model_name]["label2id"]=label2id
        topic_models[model_name]["id2label"]=id2label
        topic_models[model_name]["pad_token_label_id"]=pad_token_label_id
        topic_models[model_name]["model_type"]=model_type
        
        print ("model {} loaded".format(model_name))
    
    
    print ("{} topic models loaded".format(len(topic_models)))

    
    #load models for keyphrase extraction
    if not 'keyphrases' in conf: 
        return make_error(500, "keyphrases model information not present in config. This is a server side problem contact with the API maintainers")

    for dimension in conf['keyphrases']:
        print ("dimension: {}".format(dimension))
        model_name=dimension["name"]
        model_path=dimension["path"]

        if model_name not in available_keyphrase_models:
             print ("model {} not available".format(model_name))
             continue
         
        for i in ['short','fulltext']:
            kp_model_s_fpath=os.path.join(model_path,"single-"+i+".sav")
            kp_model_m_fpath=os.path.join(model_path,"multi-"+i+".sav")
            kp_clef_fpath=os.path.join(model_path,"clef.pkl")
            kp_clef_idf_fpath=os.path.join(model_path,"idfakCLEF.pkl")
            kp_scopus_fpath=os.path.join(model_path,"scopus.pkl")
        
            model=KeyphraseExtractor(kp_model_s_fpath, kp_model_m_fpath, kp_clef_fpath, kp_scopus_fpath, kp_clef_idf_fpath) #kp_scopus_fpath)
            #(tokenizer,model,label2id,id2label,pad_token_label_id)=load_neural_model(model_name,model_path,tmp_path)
            if model == None:
                print ("model {} could not be loaded".format(model_name))
                continue
            
            if not model_name in keyphrase_extractors:
                keyphrase_extractors[model_name]={}

            keyphrase_extractors[model_name][i]=model


    print ("{} Keyphrase extraction models loaded".format(len(keyphrase_extractors)))
    ## init logs
    #loglvl = conf['loglvl'] if 'loglvl' in conf else logging.WARNING
    #logging.basicConfig(format='%(asctime)s [%(levelname)s]: %(message)s', level=loglvl)



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
        print ("correct password")
        return True
    print ("incorrect password - {} - {} -".format(username,password))
    return False

def make_error(code, msg):
    logging.error('Error {}: {}'.format(code, msg))
    response = jsonify({
        'error': msg,
    })
    response.status_code = code
    return response


class ThematicKwordAPI(MethodResource, Resource):
    #decorators = [auth.login_required]

    @doc(description='Hercules thematic keyword API.', tags=['Hercules'])
    @use_kwargs(HerculesRequestSchema, location='json') # 'json_or_form'
    #@use_kwargs({'rotype':fields.Str(),'text':fields.Str()}, location='json') # 'json_or_form'
    @marshal_with(HerculesResponseSchema, description="returns the request object + a new property called topics, which contains a dictionary with the tags found and theis probabilities")  # marshalling with marshmallow
    def post(self, **kwargs):
        """post represent the a POST API method.

        returns the tags found and their probabilities.
        ---
        post:
              parameters:
        - in: path
             schema: HerculesRequestSchema
      responses:
        200:
          content:
            application/json:
              schema: herculesResponseSchema
        """
        #print("kwargs: {} \n \n".format(kwargs))
        json_data=request.get_json(force=True)
        response_json = json_data
        schema = HerculesRequestSchema()
        try:
            # Validate request body against schema data types
            result = schema.load(json_data)
            if 'pdf_url' not in json_data and 'title' not in json_data and 'abstract' not in json_data:
                return jsonify("error: either 'pdf_url', or 'title' and 'abstract' fields are required"), 400
        except ValidationError as err:
            # Return a nice message if validation fails
            print(err.messages, err,json_data)
            return jsonify(err.messages), 400
        
        
        print(json.dumps(json_data, indent=4, sort_keys=True))        
        
        text=""
        if 'pdf_url' in json_data:
            pdf_url_path=json_data['pdf_url']
            try:
                text=pdf_to_text(pdf_url_path)
            except Exception as e:
                return make_error(501, "'pdf_url' specified, but no text could be extracted from the given url - " + str(e))
                #return jsonify("error: 'pdf_url' specified, but no text could be extracted from the given url - {}".format(str(e))), 400
            
            logging.error('Warn: text element found in request - Text to process: _{}_  \n'.format(text))
            
        if text == "":
            logging.error('Warn {}: Could not extract the text from the pdf, trying to use other fields: \n _{}_'.format(601, text))
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
        
        logging.error('Warn {}: Text to process: \n _{}_'.format(601, text))
        text=text.replace("<br />","\n")

        if text == "": ## text is empty, no classification will be done.
            return make_error(400, "Neither could text be extracted from pdf, nor the other fields could be used, there is nothing to tag.")
        else: ## start clasifications
            ## topic classification
            topics = []
            try:                
                topics = self.classify_subject_keywords(text.replace("\n","").replace("  "," "),json_data['rotype'])
                response_json["topics"]=topics

                #logging.info("************* Final Resul formattedt: {} \n".format(topic_list))
                
            except Exception as e:
                return make_error(501, "Error when computing topic classification: " + str(e))

        return jsonify(response_json)

    def classify_subject_keywords(self,text,topic_model=None):
        results={}
        print(topic_model, topic_models.keys())
        if topic_model != None and topic_model in topic_models:
            logging.info("************ Tagging with {} model ************* \n".format(topic_model))
            model_dict=topic_models[topic_model]
            predictions, probs = evaluate(model_dict["model"], model_dict["tokenizer"], model_dict["label2id"], model_dict["pad_token_label_id"], mode="test", input_text=text,model_type=model_dict["model_type"])
            logging.info("************* Prediction ready: \n Predictions: {} \n probabilities: {}".format(predictions,probs))
            
            # we know a single sentence was tagged and its prediction are in the first position of the predictions array
            id2label=model_dict["id2label"]
            one_pred=predictions[0]
            one_prob=probs[0]

            #print(id2label,one_pred)
            #results={ id2label[str(i)]: str(f"{float(np.round(one_prob[i], 2)):.2f}") for i, k in enumerate(one_pred) if (one_prob[i] > 0.5)}
            results=[ {"word":id2label[str(i)], "porcentaje": str(f"{float(np.round(one_prob[i], 2)):.2f}")} for i, k in enumerate(one_pred) if (one_prob[i] > 0.5)]

            logging.info("************* Final Result: {} \n".format(results))
                    
            shutil.rmtree(tmp_path, ignore_errors=True)   
            os.makedirs(tmp_path)
            
        return results

## 
class SpecificKwordAPI(MethodResource, Resource):
    #decorators = [auth.login_required]

    @doc(description='Hercules Specific keyword API endpoint.', tags=['Hercules, Specific keywords'])
    @use_kwargs(HerculesKeyphraseRequestSchema, location='json') # 'json_or_form'
    #@use_kwargs({'rotype':fields.Str(),'text':fields.Str()}, location='json') # 'json_or_form'
    @marshal_with(HerculesKeyphraseResponseSchema, description="returns the request object + a new property called topics, which contains a dictionary with the tags found and theis probabilities")  # marshalling with marshmallow
    def post(self, **kwargs):
        """post represent the a POST API method.

        returns the tags found and their probabilities.
        ---
        post:
              parameters:
        - in: path
             schema: HerculesRequestSchema
      responses:
        200:
          content:
            application/json:
              schema: herculesResponseSchema
        """
        print("kwargs: {} \n \n".format(kwargs))
        json_data=request.get_json(force=True)
        response_json = json_data
        schema = HerculesKeyphraseRequestSchema()
        try:
            # Validate request body against schema data types
            result = schema.load(json_data)
        except ValidationError as err:
            # Return a nice message if validation fails
            print(err.messages, err,json_data)
            return jsonify(err.messages), 400
        
        
        print(json.dumps(json_data, indent=4, sort_keys=True))

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
        
        logging.error('WARN {}: Text to process: \n TITLE:_{}_ \n ABSTRACT:_{}_ \n TEXT:_{}_'.format(601, title, abstract, text))
        text=text.replace("<br />","\n")
        title=title.replace("<br />","\n")
        abstract=abstract.replace("<br />","\n")
        body=body.replace("<br />","\n")
        


        
        if text == "" or (title == "" and abstract == "" and body == ""): ## text is empty, no classification will be done.
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
                logging.error('WARN keyphrases:  {}'.format(keyphrases))
                term_list=[]
                for i,k in enumerate(keyphrases):
                    term_list.append({"word":k,"porcentaje":str(f"{float(np.round(keyphrases[k], 4)):.4f}")})
                response_json["topics"]=term_list
                
            except Exception as e:
                return make_error(501, "Error when extracting keyphrases: " + str(e))

        return jsonify(response_json)

        

###########################################################

#        End of API resource

###########################################################
    
api.add_resource(ThematicKwordAPI, '/thematic')#, endpoint='/topics')
docs.register(ThematicKwordAPI)

api.add_resource(SpecificKwordAPI, '/specific')#, endpoint='/topics')
docs.register(SpecificKwordAPI)



with open('openapi.json', 'w') as f:
    json.dump(docs.spec.to_dict(), f,indent=4)


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

# We're good to go! Save this to a file for now.
#with open('openapi.json', 'w') as f:
#    json.dump(spec.to_dict(), f)

if __name__ == "__main__":
    app.run()
