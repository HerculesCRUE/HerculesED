import requests
import argparse
import json
import os
import pdb

BASE_URL_DEF = "http://127.0.0.1:5000"

def test(fpath, descriptor_type, base_url):

    with open(fpath, 'r') as f:
        jreq = json.load(f)

    headers = {'Content-type': 'application/json'}
    url = os.path.join(base_url, descriptor_type)
    resp = requests.post(url, json=jreq, headers=headers)
    if resp.status_code == 200:
        print(json.dumps(resp.json(), indent=4))


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="wfcount formatutik hitzen zerrenda garbia ateratzen du.")
    parser.add_argument("JSON_PATH", help='Request parameters')
    parser.add_argument("DESCRIPTOR_TYPE", help='thematic / specific')
    parser.add_argument("--base-url", default=BASE_URL_DEF, help='Example: http://herculesapi.elhuyar.eus')
    args = parser.parse_args()

    test(args.JSON_PATH, args.DESCRIPTOR_TYPE, args.base_url)
