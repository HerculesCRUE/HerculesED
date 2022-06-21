import similarity

class MemoryROStorage(similarity.ROStorage):

    def __init__(self):
        self.ros = {}

    def connect(self) -> None:
        pass

    def add_ro(self, ro: similarity.RO) -> None:
        self.ros[ro.id] = ro

    def update_ro_ranking(self, ro: similarity.RO) -> None:
        if ro.id not in self.ros:
            raise similarity.ROIdError()
        self.ros[ro.id].ranking = ro.ranking

    def get_ro(self, ro_id) -> similarity.RO:
        if ro_id not in self.ros:
            raise similarity.ROIdError()
        return self.ros[ro_id]

    def has_ro(self, ro_id) -> bool:
        return ro_id in self.ros

    def get_embeddings(self):
        ros = []
        for db_ro in self.ros.values():
            ro = similarity.RO(db_ro.id, db_ro.type, None)
            ro._embedding = db_ro._embedding
            ros.append(ro)
        return ros
