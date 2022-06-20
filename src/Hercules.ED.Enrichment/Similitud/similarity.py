from abc import ABC, abstractmethod
from sentence_transformers import SentenceTransformer, util
import heapq
import logging
import time
import pdb

logger = logging.getLogger('SIMILARITY_API')

MODEL_ID = 'all-MiniLM-L6-v2'
RO_TYPES = ['research_paper', 'code_project', 'protocol']
RANKING_SIZE = 2

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

class RO:

    def __init__(self, ro_id, ro_type):
        
        self.id = ro_id
        self.type = ro_type
        self.text = None
        self._embedding = None
        self.authors = None
        self.thematic_descriptors = []
        self.specific_descriptors = []
        self.ranking = Ranking(size=RANKING_SIZE)

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

        
    def update_if_needed(self, new_ro: RO, distance: float):
        
        ranking = self.rankings[new_ro.type]
        if len(ranking) < self.size:
            heapq.heappush(ranking, (distance, new_ro.id))
        else:
            if distance <= ranking[0][0]:
                return False
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


    def add_ro(self, ro: RO) -> None:

        start = time.time()
        for cache_ro in self.cache.iterator():
            dist = ro.distance(cache_ro)
            #logger.debug(f"Distance ({ro.id}, {cache_ro.id}): {dist}")

            ro.ranking.update_if_needed(cache_ro, dist)
            if cache_ro.ranking.update_if_needed(ro, dist):
                self.cache.update_ro_ranking(cache_ro)
                self.db.update_ro_ranking(cache_ro)

        self.cache.add_ro(ro)
        self.db.add_ro(ro)
        end = time.time()

        logger.debug(f"add_ro elapsed time: {(end - start)}")
        

    def get_ro_ranking(self, ro_id: str, target_ro_type: str) -> list:

        ranking = self.cache.get_ranking(ro_id)
        ro_ids = ranking.get_ro_ids(target_ro_type)
        
        return ro_ids


    def generate_embedding(self, ro: RO) -> None:

        ro._embedding = self.model.encode(ro.text)

