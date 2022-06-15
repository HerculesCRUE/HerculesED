from flask import Flask, request
from flask_restful import Api, Resource
from flask_apispec import doc, use_kwargs
from flask_apispec.views import MethodResource
from flask_apispec.extension import FlaskApiSpec
from marshmallow import Schema, fields, ValidationError

import logging
import json
import pdb

logger = logging.getLogger(__name__)
logger.setLevel(logging.DEBUG)

app = Flask(__name__)
app.logger.setLevel(logging.DEBUG)
docs = FlaskApiSpec(app)
api = Api(app)

# app.config.update({
#     'APISPEC_SPEC': APISpec(
#         title='Hercules REST API for descriptors',
#         version='1.0',
#         openapi_version='3.0.0',
#         info=dict(
#             description='This module classifies papers, protocols or open-source code projects accoding to a predefined thematic descriptor taxonomy and free specific descriptors.'
#         ),
#         plugins=[MarshmallowPlugin()]
#     ),
#     'APISPEC_SWAGGER_URL': '/doc-json/',  # URI to access API Doc JSON 
#     'APISPEC_SWAGGER_UI_URL': '/doc/'  # URI to access UI of API Doc
# })


class SimilarityAddSchema(Schema):
    ro_id = fields.String(required=True, description="ID of the research object")
    ro_type = fields.String(required=True, description="Type of the research object: 'research_paper', 'code_project', 'protocol'")
    text = fields.String(required=True, description="Concatenation of title and abstract of the RO")
    authors = fields.String(required=True, description="Authors of the paper in a single string")
    thematic_descriptors = fields.String(required=True, description="List of pairs composed by thematic descriptors returned by the enrichment API and their probabilities.")
    specific_descriptors = fields.String(required=True, description="List of pairs composed by specific descriptors returned by the enrichment API and their probabilities.")

    
class SimilarityAddAPI(MethodResource, Resource):
    
    #decorators = [auth.login_required]
    @doc(description='Hercules similarity API: Add new research objects.',
         tags=['Hercules', 'similarity'])
    @use_kwargs(SimilarityAddSchema, location='json')
    #@marshal_with(SimilarityAddResponseSchema, description="")  # marshalling with marshmallow
    def post(self, **kwargs):
        print("kk")

        
api.add_resource(SimilarityAddAPI, '/add_ro')

if __name__ == "__main__":
    app.run()
