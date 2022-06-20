from abc import ABC, abstractmethod
from sentence_transformers import SentenceTransformer, util
import heapq
import logging
import time
import pdb

logger = logging.getLogger('SIMILARITY_API')

MODEL_ID = 'all-MiniLM-L6-v2'
RO_TYPES = ['research_paper', 'code_project', 'protocol']
RANKING_SIZE = 5

# Custom exceptions
class ROTypeError(Exception):
    pass

class ROIdError(Exception):
    pass


# Storage interfaces

class ROStorage(ABC):

    @abstractmethod
    def connect(self):
        pass

    @abstractmethod
    def add_ro(self, ro: "RO") -> None:
        pass

    @abstractmethod
    def update_ro_ranking(self, ro: "RO") -> None:
        pass

    @abstractmethod
    def get_ro(self, ro_id) -> "RO":
        pass

    @abstractmethod
    def has_ro(self, ro_id) -> bool:
        pass

    @abstractmethod
    def iterator(self):
        pass

    
class ROCache(ABC):

    @abstractmethod
    def add_ro(self, ro: "RO") -> None:
        pass

    @abstractmethod
    def update_ro_ranking(self, ro: "RO") -> None:
        pass

    @abstractmethod
    def get_ro(self, ro_id) -> "RO":
        pass

    @abstractmethod
    def get_ranking(self, ro_id) -> "Ranking":
        pass

    @abstractmethod
    def iterator(self):
        pass


# Entity classes

SIMILARITY_THRESHOLD = 0.0
MAX_KEYPHRASES = 5

class RO:

    def __init__(self, ro_id, ro_type, model):

        self.model = model
        self.id = ro_id
        self.type = ro_type
        self.text = None
        self._embedding = None
        self.authors = None
        self.thematic_descriptors = {
            'names': [],
            'probs': [],
            'embeddings': []
        }
        self.specific_descriptors = {
            'names': [],
            'probs': [],
            'embeddings': []
        }
        self.ranking = Ranking(size=RANKING_SIZE)


    def set_text(self, text: str) -> None:

        self.text = text
        self._embedding = self.model.encode(text)

    def set_specific_descriptors(self, names, probs):

        self.specific_descriptors['names'] = names
        self.specific_descriptors['probs'] = probs

    def encode_specific_descriptors(self):

        self.specific_descriptors['embeddings'] = self.model.encode(self.specific_descriptors['names'])

    def ro_pair_valid(self, ro: "RO") -> bool:
        
        return True

    def distance(self, ro: "RO") -> float:

        if self._embedding is None:
            logger.error(f"RO {self.id} doesn't have its embedding set")
            raise RuntimeError()
        elif ro._embedding is None:
            logger.error(f"RO {ro.id} doesn't have its embedding set")
            raise RuntimeError()

        distance_res = util.cos_sim(self._embedding, ro._embedding)
        distance = distance_res[0][0].item()

        return distance

    def explain_similarity(self, ro: "RO") -> list:

        if self._embedding is None:
            logger.error(f"RO {self.id} doesn't have its embedding set")
            raise RuntimeError()
        elif ro._embedding is None:
            logger.error(f"RO {ro.id} doesn't have its embedding set")
            raise RuntimeError()

        def get_keyphrase_score(keyphrases, text_emb):
            if len(keyphrases['names']) == 0:
                return {}
            cos_sim = util.cos_sim(keyphrases['embeddings'], text_emb)
            cos_sim = cos_sim.squeeze().tolist()
            return [ (k, c) for k, c in zip(keyphrases['names'], cos_sim) ]

        def keyphrases_by_sbert():
            text_emb = (self._embedding + ro._embedding) / 2
            result_keyphrases_ref = get_keyphrase_score(self.specific_descriptors, text_emb)
            result_keyphrases_ref = [ (k, d) for k, d in result_keyphrases_ref ]
            result_keyphrases_sim = get_keyphrase_score(ro.specific_descriptors, text_emb)
            result_keyphrases_sim = [ (k, d) for k, d in result_keyphrases_sim ]
            result_keyphrases = result_keyphrases_ref + result_keyphrases_sim
            return result_keyphrases

        def remove_similar(keyphrases):
            def are_similar(toks1, toks2):
                equal_toks = sum([ 1 for tok in toks1 if tok in toks2 ])
                equal_toks += sum([ 1 for tok in toks2 if tok in toks1 ])
                total_toks = len(toks1) + len(toks2)
                eq_rate = equal_toks / total_toks
                return eq_rate > 0.5
            similar_keyphrases = set()
            key_tokens = [ [ w if w[-1] != 's' else w[:-1] for w in keyphrase[0].lower().split() ]
                           for keyphrase in keyphrases ]
            for i in range(len(keyphrases)):
                for j in range(0, i):
                    if j in similar_keyphrases:
                        continue
                    if are_similar(key_tokens[i], key_tokens[j]):
                        similar_keyphrases.add(i)
            return [ key for i, key in enumerate(keyphrases) if i not in similar_keyphrases ]

        result_keyphrases = keyphrases_by_sbert()
        result_keyphrases.sort(key=lambda x: x[1], reverse=True)
        result_keyphrases = [ k for k in result_keyphrases if k[1] > SIMILARITY_THRESHOLD ]
        result_keyphrases = remove_similar(result_keyphrases)
        result_keyphrases = result_keyphrases[:MAX_KEYPHRASES]

        return result_keyphrases
        
        
class Ranking:

    """
    {
      'research_paper': heap([ (-dist, ro_id), ... ]),
      ...
    }
    """
    
    def __init__(self, size=10):

        self.size = size
        self.rankings = { ro_type: [] for ro_type in RO_TYPES }

        
    def update_if_needed(self, ro: RO, new_ro: RO, distance: float):
        
        ranking = self.rankings[new_ro.type]
        if len(ranking) == self.size and distance <= ranking[0][0]:
            return False

        if len(ranking) < self.size:
            heapq.heappush(ranking, (distance, new_ro.id))
        else:
            heapq.heapreplace(ranking, (distance, new_ro.id))
        return True


    def get_ro_ids(self, ro_type):

        if ro_type not in RO_TYPES:
            raise ROTypeError()
        
        ranking = self.rankings[ro_type]
        sorted_ranking = sorted(ranking, reverse=True)
        ro_ids = [ ro_id for dist, ro_id in sorted_ranking ]

        return ro_ids
                

# Application logic

class SimilarityService:

    def __init__(self, db, cache):
        
        self.db = db
        self.cache = cache
        self.model = SentenceTransformer(MODEL_ID)

        # load cache
        for ro in db.iterator():
            cache.add_ro(ro)


    def create_RO(self, ro_id, text) -> RO:
        
        return RO(ro_id, text, self.model)


    def ro_exists(self, ro_id) -> bool:

        return self.db.has_ro(ro_id)
        

    def add_ro(self, ro: RO) -> None:

        start = time.time()
        for db_ro in self.cache.iterator():
            dist = ro.distance(db_ro)
            #logger.debug(f"Distance ({ro.id}, {db_ro.id}): {dist}")

            ro.ranking.update_if_needed(ro, db_ro, dist)
            if db_ro.ranking.update_if_needed(db_ro, ro, dist):
                self.cache.update_ro_ranking(db_ro)

        self.db.add_ro(ro)
        self.cache.add_ro(ro)
        end = time.time()

        logger.debug(f"add_ro elapsed time: {(end - start):.2f}")
        

    def get_ro_ranking(self, ro_id: str, target_ro_type: str) -> list:

        ranking = self.cache.get_ranking(ro_id)
        ro_ids = ranking.get_ro_ids(target_ro_type)
        similarity_keys = []
        
        ro = self.db.get_ro(ro_id)
        ro.encode_specific_descriptors()
        for sim_ro_id in ro_ids:
            sim_ro = self.db.get_ro(sim_ro_id)
            sim_ro.encode_specific_descriptors()
            similarity_keys.append(ro.explain_similarity(sim_ro))
            
        return list(zip(ro_ids, similarity_keys))
