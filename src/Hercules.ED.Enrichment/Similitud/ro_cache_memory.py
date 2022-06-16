import similarity

class MemoryROCache(similarity.ROCache):

    def __init__(self):
        pass

    def add_ro(self, ro: similarity.RO) -> None:
        pass

    def update_ro_ranking(self, ro: similarity.RO) -> None:
        pass

    def get_ro(self, ro_id) -> similarity.RO:
        pass

    def get_ranking(self, ro_id) -> similarity.Ranking:
        pass

    def iterator(self):
        pass
