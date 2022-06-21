import similarity

class MemoryROCache(similarity.ROCache):

    def __init__(self):
        self.ros = {}

    def add_ro(self, ro: similarity.RO) -> None:
        self.ros[ro.id] = {
            'id': ro.id,
            'type': ro.type,
            'embedding': ro._embedding,
            'ranking': ro.ranking,
        }

    def update_ro_ranking(self, ro: similarity.RO) -> None:
        if ro.id not in self.ros:
            raise similarity.ROIdError()
        self.ros[ro.id]['ranking'] = ro.ranking

    def get_ro(self, ro_id) -> similarity.RO:
        if ro_id not in self.ros:
            raise similarity.ROIdError()
        return self._to_ro(self.ros[ro_id])

    def get_ranking(self, ro_id) -> similarity.Ranking:
        if ro_id not in self.ros:
            raise similarity.ROIdError()
        return self.ros[ro_id]['ranking']

    def iterator(self):
        for ro_dic in self.ros.values():
            ro = self._to_ro(ro_dic)
            yield ro

    def clear(self):
        self.ros.clear()

    @staticmethod
    def _to_ro(ro_dic):
        ro = similarity.RO(ro_dic['id'], ro_dic['type'], None)
        ro._embedding = ro_dic['embedding']
        ro.ranking = ro_dic['ranking']
        return ro
