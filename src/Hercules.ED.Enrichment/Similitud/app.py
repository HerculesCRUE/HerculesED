from flask import Flask, request, jsonify, Response
from flask_restful import Api, Resource
from flask_apispec import doc, use_kwargs
from flask_apispec.views import MethodResource
from flask_apispec.extension import FlaskApiSpec
from apispec import APISpec
from apispec.ext.marshmallow import MarshmallowPlugin
from marshmallow import Schema, fields, ValidationError

from similarity import SimilarityService, RO, Ranking, ROIdError, ROTypeError
import ro_storage_memory
import ro_storage_mongodb
import ro_cache_memory

import logging
import json
import os
import pdb

logging.basicConfig()
logger = logging.getLogger('SIMILARITY_API')
logger.setLevel(logging.DEBUG)

app = Flask(__name__)
app.logger.setLevel(logging.DEBUG)
api = Api(app)

app.config.update({
    'APISPEC_SPEC': APISpec(
        title='Hercules REST API to retrieve similar ROs',
        version='0.1',
        openapi_version='3.0.0',
        info=dict(
            description='This module receives a collection of ROs. It then returns the most similar ROs of a given RO.'
        ),
        plugins=[MarshmallowPlugin()]
    ),
    'APISPEC_SWAGGER_URL': '/doc-json/',  # URI to access API Doc JSON 
    'APISPEC_SWAGGER_UI_URL': '/doc/',  # URI to access UI of API Doc
    'PROPAGATE_EXCEPTIONS': True,
})
docs = FlaskApiSpec(app)

# config

def load_conf():
    
    CONFDIR = os.path.dirname(os.path.realpath(__file__))
    with open('{}/conf.json'.format(CONFDIR), 'r', encoding='utf-8') as f:
        conf = json.load(f)

    fields = ['device', 'model']
    for fieldname in fields:
        if fieldname not in conf:
            raise Exception(f"Field '{fieldname}' is missing in configuration JSON")

    return conf

conf = load_conf()


# Type definitions

class ROSchema(Schema):
    ro_id = fields.String(required=True, description="ID of the research object")
    ro_type = fields.String(required=True, description="Type of the research object: 'research_paper', 'code_project', 'protocol'")
    text = fields.String(required=True, description="Concatenation of title and abstract of the RO")
    authors = fields.List(fields.String, required=True, description="Authors of the paper in a single string")
    thematic_descriptors = fields.List(fields.List(fields.Raw), required=True, description="List of pairs composed by thematic descriptors returned by the enrichment API and their probabilities.")
    specific_descriptors = fields.List(fields.List(fields.Raw), required=True, description="List of pairs composed by specific descriptors returned by the enrichment API and their probabilities.")
    update_ranking = fields.Bool(required=False, default=True, description="Don't update rankings and cache if False. Useful for batch loading. RebuildRankings must be called after loading ROs with update_ranking=False.")
    
class ROIdSchema(Schema):
    ro_id = fields.String(required=True, description="ID of the research object")
    
class BatchSchema(Schema):
    batch = fields.List(fields.Nested(ROSchema))
    
class QuerySchema(Schema):
    ro_id = fields.String(required=True, description="ID of the research object")
    ro_type_target = fields.String(required=True, description="Type of the similar research objects to return: 'research_paper', 'code_project', 'protocol'")

def validate_descriptors(jdoc):
    def validate_descriptor_type(dtype):
        if dtype in jdoc:
            for desc in jdoc[dtype]:
                if len(desc) != 2 or type(desc[0]) != str or type(desc[1]) != float:
                    raise ValueError(f"'{dtype}' argument is not valid")
    validate_descriptor_type('thematic_descriptors')
    validate_descriptor_type('specific_descriptors')
    
# Service-level object instances

db = ro_storage_mongodb.MongoROStorage()
cache = ro_cache_memory.MemoryROCache()
similarity = SimilarityService(db, cache, conf['model'], conf['device'])

# Endpoint classes

class RO_API(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Update a research object.',
         tags=['Hercules', 'similarity'])
    @use_kwargs(ROSchema, location='json')
    def put(self, **kwargs):
        logger.debug(kwargs)
        try:
            validate_descriptors(kwargs)
        except ValueError as e:
            resp_json = '{"error_msg": "' + str(e) + '"}'
            return Response(response=resp_json, status=422, mimetype='application/json')
        ro = similarity.create_RO(kwargs['ro_id'], kwargs['ro_type'])
        ro.set_text(kwargs['text'], similarity.model)
        ro.authors = kwargs['authors']
        names = [ n for n, p in kwargs['specific_descriptors'] ]
        probs = [ p for n, p in kwargs['specific_descriptors'] ]
        ro.set_specific_descriptors(names, probs)
        new_created = similarity.upsert_ro(ro)
        status_code = 201 if new_created else 200
        return Response(response="", status=status_code, mimetype='application/json')
    
    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Delete a research object.',
         tags=['Hercules', 'similarity'])
    @use_kwargs(ROIdSchema, location='query')
    def delete(self, **kwargs):
        logger.debug(kwargs)
        similarity.delete_ro(kwargs['ro_id'])
        return jsonify()

    
class ROCollection_API(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Add a batch of research objects.',
         tags=['Hercules', 'similarity'])
    @use_kwargs(BatchSchema, location='json')
    def post(self, **kwargs):
        batch = []
        for ro_kwargs in kwargs['batch']:
            try:
                validate_descriptors(ro_kwargs)
            except ValueError as e:
                resp_json = '{"error_msg": "' + str(e) + '"}'
                return Response(response=resp_json, status=422, mimetype='application/json')
            ro = similarity.create_RO(ro_kwargs['ro_id'], ro_kwargs['ro_type'])
            ro.text = ro_kwargs['text']
            ro.authors = ro_kwargs['authors']
            names = [ n for n, p in ro_kwargs['specific_descriptors'] ]
            probs = [ p for n, p in ro_kwargs['specific_descriptors'] ]
            ro.set_specific_descriptors(names, probs)
            batch.append(ro)
        similarity.encode_batch(batch)
        similarity.add_ros(batch, update_ranking=False)
        logger.debug("Batch of ROs added")
        return jsonify()

    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Retrieve all RO ids.',
         tags=['Hercules', 'similarity'])
    @use_kwargs({'ro_type_target': fields.String()}, location='query')
    def get(self, **kwargs):
        ids = similarity.get_ro_ids(kwargs['ro_type_target'])
        return jsonify(ids)
    
        
class RebuildRankings_API(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Rebuilds cache and all the rankings of similar ROs.',
         tags=['Hercules', 'similarity'])
    @use_kwargs({}, location='json')
    def post(self, **kwargs):
        logger.debug(kwargs)
        similarity.rebuild_cache()
        return jsonify()

        
class Similar_API(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Query similar ROs.',
         tags=['Hercules', 'similarity'])
    @use_kwargs(QuerySchema, location='query')
    def get(self, **kwargs):
        logger.debug(kwargs)
        similar_ro_ids = similarity.get_ro_ranking(kwargs['ro_id'], kwargs['ro_type_target'])            
        logger.debug(f"Similar ROs: {similar_ro_ids}")
        return jsonify({ 'similar_ros': similar_ro_ids })


# Declare endpoints and documentation for the API

api.add_resource(RO_API, '/ro')
api.add_resource(ROCollection_API, '/ro-collection')
api.add_resource(RebuildRankings_API, '/rebuild-rankings')
api.add_resource(Similar_API, '/similar')

docs.register(RO_API)
docs.register(ROCollection_API)
docs.register(RebuildRankings_API)
docs.register(Similar_API)


# Error handlers

@app.errorhandler(ROIdError)
def handle_bad_request(e):
    return Response(response='{"error_msg": "RO not found"}',
                    status=404,
                    mimetype='application/json')

@app.errorhandler(ROTypeError)
def handle_bad_request(e):
    return Response(response='{"error_msg": "Invalid RO type"}',
                    status=400,
                    mimetype='application/json')

if __name__ == "__main__":
    app.run()
