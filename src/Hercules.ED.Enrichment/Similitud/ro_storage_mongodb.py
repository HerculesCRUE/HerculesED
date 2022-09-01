import similarity
import pdb
from pymongo import MongoClient

MONGO_HOST = 'mongodb'
MONGO_PORT = 27017

class MongoROStorage(similarity.ROStorage):

    def __init__(self):
        pass

    def connect(self) -> None:
        self.client = MongoClient(MONGO_HOST, port=MONGO_PORT)
        self.db = self.client.hercules_similarity

    def add_ro(self, ro: similarity.RO) -> None:
        bro = self.ro_to_bson(ro)
        self.db.ro.insert_one(bro)

    def delete_ro(self, ro_id: str) -> None:
        if not self.has_ro(ro_id):
            raise similarity.ROIdError()
        self.db.ro.delete_one({'_id': ro_id})

    def update_ro(self, ro: similarity.RO) -> None:
        bro = self.ro_to_bson(ro)
        self.db.ro.replace_one({'_id': ro.id}, bro)
                
    def add_ros(self, ros) -> None:
        bros = [ self.ro_to_bson(ro) for ro in ros ]
        self.db.ro.insert_many(bros)

    def get_ro(self, ro_id) -> similarity.RO:
        bro = self.db.ro.find_one({'_id': ro_id})
        if bro is None:
            raise similarity.ROIdError()
        return self.bson_to_ro(bro)
    
    def get_ro_ids(self, ro_type) -> list:
        ro_cursor = self.db.ro.find({'type': ro_type}, {'_id':1})
        return [ bro['_id'] for bro in ro_cursor ]
    
    def has_ro(self, ro_id) -> bool:
        return self.db.ro.find_one({'_id': ro_id}) is not None

    def get_embeddings(self):
        """ROs with id, type, embedding"""
        ro_cursor = self.db.ro.find({}, {'_id':1, 'type':1, 'embedding':1})
        return [ self.bson_to_ro(bro) for bro in ro_cursor ]

    @staticmethod
    def ro_to_bson(ro):
        jro = similarity.SimilarityService.ro_to_json(ro)
        jro['_id'] = jro.pop('ro_id')
        jro['type'] = jro.pop('ro_type')
        if 'specific_descriptors' in jro:
            specific = jro.pop('specific_descriptors')
            names, probs = [ d[0] for d in specific ], [ d[1] for d in specific ]
            jro['specific_descriptors'] = {
                'names': names,
                'probs': probs,
                'embeddings': [],
            }
        if 'thematic_descriptors' in jro:
            thematic = jro.pop('thematic_descriptors')
            names, probs = [ d[0] for d in thematic ], [ d[1] for d in thematic ]
            jro['thematic_descriptors'] = {
                'names': names,
                'probs': probs,
                'embeddings': [],
            }
        return jro

    @staticmethod
    def bson_to_ro(bro):
        bro['ro_id'] = bro.pop('_id')
        bro['ro_type'] = bro.pop('type')
        if 'thematic_descriptors' in bro:
            thematic = bro.pop('thematic_descriptors')
            bro['thematic_descriptors'] = list(zip(thematic['names'], thematic['probs']))
        if 'specific_descriptors' in bro:
            specific = bro.pop('specific_descriptors')
            bro['specific_descriptors'] = list(zip(specific['names'], specific['probs']))
        return similarity.SimilarityService.json_to_ro(bro)
        
