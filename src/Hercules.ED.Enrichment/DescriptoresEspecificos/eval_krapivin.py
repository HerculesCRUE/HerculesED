import argparse
import pandas as pd
import numpy as np
import statistics as stats
import joblib
import random
import pdb

from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report, confusion_matrix, f1_score
from sklearn.ensemble import GradientBoostingClassifier
from imblearn.over_sampling import RandomOverSampler

import keyphrase_extraction as kpex
import train_eval_krapivin

SHOW_IMPORTANCES = False


def main(model_s_fpath, model_m_fpath, test_fpath):

    df = pd.read_csv(test_fpath, sep="\t")
    df = kpex.clean_dataset(df)
    df_s, df_m = kpex.split_single_multi(df)

    model_s, model_m = None, None
    X_s, X_m, y_s, y_m = None, None, None, None
    
    if model_s_fpath is not None:
        X_s, y_s = kpex.split_feats_label(df_s)
        model_s = joblib.load(model_s_fpath)
    if model_m_fpath is not None:
        X_m, y_m = kpex.split_feats_label(df_m)
        model_m = joblib.load(model_m_fpath)

    if SHOW_IMPORTANCES:
        print("--- Attribute importances ---")
        for attr, imp in zip(X_m.columns[2:], model_m.feature_importances_):
            print(f"{attr}: {imp:.4f}")
        print()
        
    evaluate_models(model_s, model_m, X_s, X_m, y_s, y_m)

    
def evaluate_models(model_s, model_m, X_s, X_m, y_s, y_m):
    # Evaluate models
    
    if model_s is not None:
        rankings = _create_rankings(model_s, model_m, X_s, y_s)
        print("--- SINGLE-WORD MODEL ---")
        _evaluate(rankings['IS_KEYWORD?'].to_numpy(),
                  rankings['PROB'].to_numpy(),
                  rankings['DOCID'].to_numpy())

    if model_m is not None:
        rankings = _create_rankings(model_s, model_m, X_m, y_m)
        print("\n--- MULTIWORD MODEL ---")
        _evaluate(rankings['IS_KEYWORD?'].to_numpy(),
                  rankings['PROB'].to_numpy(),
                  rankings['DOCID'].to_numpy())
        
    if model_s is not None and model_m is not None:
        X = pd.concat([X_s, X_m])
        y = np.concatenate([y_s, y_m])
        rankings = _create_rankings(model_s, model_m, X, y)
        print("\n--- MERGED ---")
        _evaluate(rankings['IS_KEYWORD?'].to_numpy(),
                  rankings['PROB'].to_numpy(),
                  rankings['DOCID'].to_numpy())
    

def _create_rankings(model_s, model_m, X, y):

    data = X.assign(IS_KEYWORD=y)
    
    rankings = []
    for doc_id, doc_df in data.groupby('DOCID'):
        X = doc_df.drop(['IS_KEYWORD'], axis=1)
        y = doc_df['IS_KEYWORD']
        ranking = kpex.KeyphraseExtractor.create_ranking(model_s, model_m, X, y=y)
        ranking['DOCID'] = doc_id
        
        rankings.append(ranking)

    return pd.concat(rankings)

    
def _evaluate(y_true, y_pred_prob, docids):

    y_pred = np.round(y_pred_prob)

    print(classification_report(y_true, y_pred))
    print("R@5\t{:.3f}".format(_scores_at(5, docids, y_true, y_pred_prob)[1]))
    print("R@10\t{:.3f}".format(_scores_at(10, docids, y_true, y_pred_prob)[1]))
    print("R@15\t{:.3f}".format(_scores_at(15, docids, y_true, y_pred_prob)[1]))
    print("R@20\t{:.3f}".format(_scores_at(20, docids, y_true, y_pred_prob)[1]))
    

def _scores_at(k, doc_ids, y_true, y_pred):

    data = list(zip(doc_ids, y_true, y_pred))

    data_by_docs = {}  # doc_id -> [(y_true, y_pred)]
    for e in data:
        if e[0] not in data_by_docs:
            data_by_docs[e[0]] = []
        data_by_docs[e[0]].append( (e[1], e[2]) )
        
    for doc_id, entries in data_by_docs.items():
        data_by_docs[doc_id].sort(key=lambda e: e[1], reverse=True)
        # random.shuffle(data_by_docs[doc_id])  # useful to evaluate random rankings

    docs_sorted = [ ([ e[0] for e in entries ]) for entries in data_by_docs.values() ]
    
    scores = []
    for doc_sorted in docs_sorted:
        tp = sum(doc_sorted[:k])
        fp = len(doc_sorted[:k]) - tp
        fn = sum(doc_sorted[k:])
        scores.append( (tp, fp, fn) )
    
    tp_tot = sum([ s[0] for s in scores ])
    fp_tot = sum([ s[1] for s in scores ])
    fn_tot = sum([ s[2] for s in scores ])
    p_mic = tp_tot / (tp_tot + fp_tot)
    r_mic = tp_tot / (tp_tot + fn_tot)
    p_mac = stats.mean([ tp / (tp + fp) for tp, fp, fn in scores if tp + fp > 0 ])
    r_mac = stats.mean([ tp / (tp + fn) for tp, fp, fn in scores if tp + fn > 0 ])

    return p_mic, r_mic


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="")
    parser.add_argument("--single", "-s", required=False, help='Krapivin trained model (single-word)')
    parser.add_argument("--multi", "-m", required=False, help='Krapivin trained model (multi-word)')
    parser.add_argument("TEST_TSV", help='Krapivin test TSV file')
    args = parser.parse_args()

    if args.single is None and args.multi is None:
        raise ValueError("At least one of --single or --multi must be provided")

    main(args.single, args.multi, args.TEST_TSV)
