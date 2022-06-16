import argparse
import os
import pdb

import pandas as pd

N_TRAIN = 300000
N_TEST = 50
MIN_LEN = 300
RANDOM_SEED = 4


def main(tsv_fpath):

    data = pd.read_csv(tsv_fpath, sep='\t')

    data = data[data.text.str.len() > MIN_LEN]
    data = data.sample(frac=1.0, random_state=RANDOM_SEED)

    test = data.iloc[:N_TEST, :]
    train = data.iloc[N_TEST:(N_TEST+N_TRAIN), :]

    fname_base, ext = os.path.splitext(tsv_fpath)
    train_fpath = fname_base+'.train'+ext
    test_fpath = fname_base+'.test'+ext
        
    train.to_csv(train_fpath, sep='\t', index=False)
    test.to_csv(test_fpath, sep='\t', index=False)


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="Split dataset into two subsets. The test-set must contain N rows, and the rest will compose the train-set.")
    parser.add_argument("TSV", help='TSV file of the full dataset (id, text)')
    args = parser.parse_args()

    main(args.TSV)
