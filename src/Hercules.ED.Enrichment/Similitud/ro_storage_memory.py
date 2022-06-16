import similarity

class MemoryROStorage(similarity.ROStorage):

    def __init__(self):
        pass

    def connect(self) -> None:
        pass

    def add_ro(self, ro: similarity.RO) -> None:
        pass

    def update_ro_ranking(self, ro: similarity.RO) -> None:
        pass

    def get_ro(self, ro_id) -> similarity.RO:
        pass

    def iterator(self):
        pass
