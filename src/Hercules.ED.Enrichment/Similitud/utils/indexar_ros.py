import argparse
import requests
import json
import logging
import tqdm
import math
import pdb
import os

logging.basicConfig(level=logging.INFO)

BATCH_SIZE = 100
URL_BASE = "http://herculesapi.elhuyar.eus/similarity"

def index_ros(data_fpath, ro_id):

    ro = None
    with open(data_fpath, 'r') as f:
        ros = json.load(f)

    if ro_id is not None:
        ro = next((ro for ro in ros if ro['ro_id'] == ro_id), None)
        if ro is None:
            raise ValueError(f"RO {ro_id} not found")
        ros = [ ro ]

    def batch(iterable, n=1):
        l = len(iterable)
        for ndx in range(0, l, n):
            yield iterable[ndx:min(ndx + n, l)]

    URL_add_batch = URL_BASE + '/add_batch'
    URL_rebuild = URL_BASE + '/rebuild_rankings'
    
    try:
        for ros in tqdm.tqdm(batch(ros, n=BATCH_SIZE), total=math.ceil(len(ros)/BATCH_SIZE)):
            r = requests.post(URL_add_batch, json={"batch": ros})
            if r.status_code != 200:
                raise Exception(f"Error adding ROs. http-status:{r.status_code}")
            logging.debug("Batch added")

        r = requests.post(URL_rebuild, json={})
        if r.status_code != 200:
            logging.error(f"Error rebuilding RO rankings. http-status:{r.status_code}")
    except Exception as e:
        logging.error(str(e))
        exit()
    

if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="")
    parser.add_argument("DATA", help='Dataset fila in JSON format')
    parser.add_argument("--roid", required=False, help='Insert only the RO with this ID')
    args = parser.parse_args()

    index_ros(args.DATA, args.roid)
