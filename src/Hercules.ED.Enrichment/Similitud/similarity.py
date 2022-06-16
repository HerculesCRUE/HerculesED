from abc import ABC, abstractmethod
import logging

RO_TYPES = ['research_paper', 'code_project', 'protocol']

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

    def __init__(self, ro_id):
        
        self.id = ro_id
        self.type = None
        self.text = None
        self.embedding = None
        self.authors = None
        self.thematic_descriptors = []
        self.specific_descriptors = []

    def ro_pair_valid(self, ro: "RO") -> bool:
        pass

    def distance(self, ro: "RO") -> float:
        pass
        
    
class Ranking:

    def __init__(self):
        self.rankings = { ro_type: [] for ro_type in RO_TYPES }


# Application logic

def cache_load(ro_db: ROStorage) -> ROCache:

    pass


def add_ro(ro: RO, ro_db: ROStorage, ro_cache: ROCache) -> None:

    def update_ro_ranking(similar_ro: RO) -> None:
        pass

    pass


def get_ro_ranking(ro_id: str, target_ro_type: str, ro_db: ROStorage) -> list:

    pass



