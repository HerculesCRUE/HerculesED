import argparse
import pandas as pd
import numpy as np
import statistics as stats
import joblib
import pdb

from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report, confusion_matrix, f1_score
from sklearn.ensemble import GradientBoostingClassifier
from imblearn.over_sampling import RandomOverSampler

import keyphrase_extraction as kpex
import train_eval_krapivin


def main(model_s_fpath, model_m_fpath, test_fpath, doc_id):

    model_s = joblib.load(model_s_fpath)
    model_m = joblib.load(model_m_fpath)

    df = pd.read_csv(test_fpath, sep="\t")
    df = df[df.DOCID==doc_id]

    X, y = kpex.split_feats_label(df)
    ranking = kpex.KeyphraseExtractor.create_ranking(model_s, model_m, X, y)
    ranking_s, ranking_m = kpex.split_single_multi(ranking)
    
    dump_ranking(ranking, "MERGED")
    dump_ranking(ranking_s, "SINGLE-WORD")
    dump_ranking(ranking_m, "MULTIWORD")
    

def dump_ranking(ranking, title):

    print("> " + title)
    for i in ranking.index:
        kw = ranking.at[i, 'KW']
        prob = ranking.at[i, 'PROB']
        y_true = ranking.at[i, 'IS_KEYWORD?']
        print('\t'.join([kw, str(prob), str(y_true)]))
    print()
    

def rank(model_s, model_m, test_fpath, doc_id):

    df = pd.read_csv(test_fpath, sep="\t")
    df = df[df.DOCID==doc_id]

    X, y = kpex.split_feats_label(df)
    ranking = kpex.KeyphraseExtractor.create_ranking(model_s, model_m, X, y)
    
    return kw_s_probs, kw_m_probs


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="")
    parser.add_argument("MODEL_SINGLE", help='Krapivin trained model (single-word)')
    parser.add_argument("MODEL_MULTI", help='Krapivin trained model (multiword)')
    parser.add_argument("TEST_TSV", help='Krapivin test TSV file')
    parser.add_argument("DOC_ID", help='Use only this document from test-set')
    args = parser.parse_args()

    main(args.MODEL_SINGLE, args.MODEL_MULTI, args.TEST_TSV, args.DOC_ID)
