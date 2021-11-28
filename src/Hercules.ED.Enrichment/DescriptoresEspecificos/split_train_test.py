import argparse
import pandas as pd
import os
import pdb


def main(krapivin_fpath, train_docs_fpath, test_docs_fpath):

    with open(train_docs_fpath, 'r') as f:
        text = f.read()
        train_docs = set(text.split())
    with open(test_docs_fpath, 'r') as f:
        text = f.read()
        test_docs = set(text.split())
        
    df = pd.read_csv(krapivin_fpath, sep="\t")
    
    train_df = df[df.DOCID.isin(train_docs)]
    test_df = df[df.DOCID.isin(test_docs)]
    dev_df = df[~df.DOCID.isin(set.union(train_docs, test_docs))]

    fname_base, ext = os.path.splitext(krapivin_fpath)
    train_fpath = fname_base+'.train'+ext
    test_fpath = fname_base+'.test'+ext
    dev_fpath = fname_base+'.dev'+ext

    train_df.to_csv(train_fpath, sep='\t', index=False)
    test_df.to_csv(test_fpath, sep='\t', index=False)
    dev_df.to_csv(dev_fpath, sep='\t', index=False)
    

if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="")
    parser.add_argument("KRAPIVIN_TSV", help='Krapivin TSV file')
    parser.add_argument("TRAIN_DOCIDS", help='A text file containing a list of doc-ids')
    parser.add_argument("TEST_DOCIDS", help='A text file containing a list of doc-ids')
    args = parser.parse_args()

    main(args.KRAPIVIN_TSV, args.TRAIN_DOCIDS, args.TEST_DOCIDS)
