import requests
import argparse
import json
import pdb


def test(fpath, descriptor_type):

    with open(fpath, 'r') as f:
        jreq = json.load(f)

    headers = {'Content-type': 'application/json'}
    resp = requests.post(f"http://127.0.0.1:5000/{descriptor_type}", json=jreq, headers=headers)
    if resp.status_code == 200:
        print(json.dumps(resp.json(), indent=4))


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="wfcount formatutik hitzen zerrenda garbia ateratzen du.")
    parser.add_argument("JSON_PATH", help='Request parameters')
    parser.add_argument("DESCRIPTOR_TYPE", help='thematic / specific')
    args = parser.parse_args()

    test(args.JSON_PATH, args.DESCRIPTOR_TYPE)
