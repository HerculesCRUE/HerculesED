import similarity

class MemoryROStorage(similarity.ROStorage):

    def __init__(self):
        self.ros = {}

    def connect(self) -> None:
        pass

    def add_ro(self, ro: similarity.RO) -> None:
        self.ros[ro.id] = ro

    def update_ro_ranking(self, ro: similarity.RO) -> None:
        self.ros[ro.id].ranking = ro.ranking

    def get_ro(self, ro_id) -> similarity.RO:
        return self.ros[ro_id]

    def iterator(self):
        return self.ros.values()
