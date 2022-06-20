import argparse
import requests
import json
import logging
import tqdm
import pdb

logging.basicConfig()


def index_ros(data_fpath, ro_id):

    ro = None
    with open(data_fpath, 'r') as f:
        ros = json.load(f)

    if ro_id is not None:
        ro = next((ro for ro in ros if ro['ro_id'] == ro_id), None)
        if ro is None:
            raise ValueError(f"RO {ro_id} not found")
        ros = [ ro ]

    URL_add = 'http://localhost:5000/add_ro'
    URL_rebuild = 'http://localhost:5000/rebuild_rankings'

    try:
        for ro in tqdm.tqdm(ros):
            ro['update_ranking'] = False
            r = requests.post(URL_add, json=ro)
            if r.status_code != 200:
                logging.error(f"Error adding RO {ro['ro_id']}. http-status:{r.status_code}")

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
