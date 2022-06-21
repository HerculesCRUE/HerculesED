import argparse
import pandas as pd
import random
import json
import pdb

def extract_subset(test_fpath, train_fpath, ranks_fpath, size, out_fpath):

    with open(test_fpath, 'r') as f:
        test_jdoc = json.load(f)
    with open(train_fpath, 'r') as f:
        train_jdoc = json.load(f)
    ranks_df = pd.read_csv(ranks_fpath, delimiter='\t')

    similar_ids = set(ranks_df.sim_doc_id.values)

    result_jdoc = []
    result_jdoc.extend(test_jdoc)
    result_jdoc.extend([ ro for ro in train_jdoc if ro['ro_id'] in similar_ids ])
    for ro in random.sample(train_jdoc, size):
        if ro['ro_id'] not in similar_ids:
            result_jdoc.append(ro)
            if len(result_jdoc) >= size:
                break

    random.shuffle(result_jdoc)
    with open(out_fpath, 'w') as f:
        json.dump(result_jdoc, f, indent=4)


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="Extracts a subset of ROs given a test set and a train set and the rankings of the most similars of the ROs in the test set taken from the train set. It selects the most similar ROs (from the rankings) and fills the new subset with other random ROs.")
    parser.add_argument("TEST", help='Dataset file in JSON format')
    parser.add_argument("TRAIN", help='Dataset file in JSON format')
    parser.add_argument("RANKINGS", help='Rankings of the ROs in test set in TSV format')
    parser.add_argument("SIZE", type=int, help='Number of ROs in the subset to create')
    parser.add_argument("OUT", help='File path of the output JSON')
    args = parser.parse_args()

    extract_subset(args.TEST, args.TRAIN, args.RANKINGS, args.SIZE, args.OUT)
