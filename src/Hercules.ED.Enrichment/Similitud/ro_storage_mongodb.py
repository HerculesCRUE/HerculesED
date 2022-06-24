import similarity
import numpy as np
import pdb
from pymongo import MongoClient

class MongoROStorage(similarity.ROStorage):

    def __init__(self):
        pass

    def connect(self) -> None:
        self.client = MongoClient('127.0.0.1', port=27017)
        self.db = self.client.hercules_similarity

    def add_ro(self, ro: similarity.RO) -> None:
        jro = self.ro_to_json(ro)
        self.db.ro.insert_one(jro)

    def delete_ro(self, ro_id: str) -> None:
        if not self.has_ro(ro_id):
            raise similarity.ROIdError()
        self.db.ro.delete_one({'_id': ro_id})
                
    def add_ros(self, ros) -> None:
        jros = [ self.ro_to_json(ro) for ro in ros ]
        self.db.ro.insert_many(jros)

    def get_ro(self, ro_id) -> similarity.RO:
        jro = self.db.ro.find_one({'_id': ro_id})
        if jro is None:
            raise similarity.ROIdError()
        return self.json_to_ro(jro)

    def has_ro(self, ro_id) -> bool:
        return self.db.ro.find_one({'_id': ro_id}) is not None

    def get_embeddings(self):
        """ROs with id, type, embedding"""
        ro_cursor = self.db.ro.find({}, {'_id':1, 'type':1, 'embedding':1})
        return [ self.json_to_ro(jro) for jro in ro_cursor ]
        
    @staticmethod
    def ro_to_json(ro: similarity.RO) -> dict:
        jro = {
            '_id': ro.id,
            'type': ro.type,
            'text': ro.text,
            'embedding': ro._embedding.tolist(),
            'authors': ro.authors,
            'thematic_descriptors': ro.thematic_descriptors,
            'specific_descriptors': ro.specific_descriptors,
        }
        return jro

    @staticmethod
    def json_to_ro(jro: dict) -> similarity.RO:
        ro = similarity.RO(jro['_id'], jro['type'])
        if 'text' in jro:
            ro.text = jro['text']
        if 'embedding' in jro:
            ro._embedding = np.array(jro['embedding'], dtype=np.float32)
        if 'authors' in jro:
            ro.authors = jro['authors']
        if 'thematic_descriptors' in jro:
            ro.thematic_descriptors = jro['thematic_descriptors']
        if 'specific_descriptors' in jro:
            ro.specific_descriptors = jro['specific_descriptors']
        return ro
