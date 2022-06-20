import argparse
import json, csv

def tsv_to_json(tsv_fpath, ro_type, json_fpath):

    json_data = []
    
    with open(tsv_fpath) as f:
        csv_reader = csv.DictReader(f, delimiter='\t')  # default params
        fields = csv_reader.fieldnames
        for row in csv_reader:
            ro = {
                'ro_id': row['id'],
                'ro_type': ro_type,
                'text': row['text'],
                'authors': "",
                'thematic_descriptors': [],
                'specific_descriptors': [],
            }
            json_data.append(ro)

    with open(json_fpath, 'w') as f:
        json.dump(json_data, f, indent=4)
            

if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="")
    parser.add_argument("TSV", help='TSV file with RO data (id, text)')
    parser.add_argument("RO_TYPE", help='RO type: research_paper, code_project, protocol')
    parser.add_argument("OUT", help='Output JSON file path')
    args = parser.parse_args()

    tsv_to_json(args.TSV, args.RO_TYPE, args.OUT)
