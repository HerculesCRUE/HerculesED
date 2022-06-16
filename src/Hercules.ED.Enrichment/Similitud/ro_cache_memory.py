import similarity

class MemoryROCache(similarity.ROCache):

    def __init__(self):
        self.ros = {}

    def add_ro(self, ro: similarity.RO) -> None:
        self.ros[ro.id] = {
            'id': ro.id,
            'embedding': ro.embedding,
            'ranking': ro.ranking,
        }

    def update_ro_ranking(self, ro: similarity.RO) -> None:
        self.ros[ro.id].ranking = ro.ranking

    def get_ro(self, ro_id) -> similarity.RO:
        return self.ros[ro_id]

    def get_ranking(self, ro_id) -> similarity.Ranking:
        return self.ros[ro_id].ranking

    def iterator(self):
        return self.ros.values()
