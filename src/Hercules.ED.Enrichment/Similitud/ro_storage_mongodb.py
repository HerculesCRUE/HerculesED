import similarity
import pdb
from pymongo import MongoClient

class MongoROStorage(similarity.ROStorage):

    def __init__(self):
        pass

    def connect(self) -> None:
        self.client = MongoClient('127.0.0.1', port=27017)
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
        return jro

    @staticmethod
    def bson_to_ro(bro):
        bro['ro_id'] = bro.pop('_id')
        bro['ro_type'] = bro.pop('type')
        return similarity.SimilarityService.json_to_ro(bro)
        
