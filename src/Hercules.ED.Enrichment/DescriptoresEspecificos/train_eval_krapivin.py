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
import eval_krapivin


def main(train_fpath, test_fpath, is_fulltext, kw_type):

    # Load data
    
    df_train = pd.read_csv(train_fpath, sep="\t")
    df_test = pd.read_csv(test_fpath, sep="\t")

    df_train = kpex.clean_dataset(df_train)
    df_test = kpex.clean_dataset(df_test)
    df_s_train, df_m_train = kpex.split_single_multi(df_train)
    df_s_test, df_m_test = kpex.split_single_multi(df_test)
    
    if kw_type != 'MULTI':
        X_s_train, y_s_train = kpex.split_feats_label(df_s_train)
        X_s_train = kpex.select_features(X_s_train, 'SINGLE', is_fulltext)
        X_s_train, y_s_train = RandomOverSampler(random_state=65).fit_resample(X_s_train, y_s_train)
        X_s_test, y_s_test = kpex.split_feats_label(df_s_test)
    if kw_type != 'SINGLE':
        X_m_train, y_m_train = kpex.split_feats_label(df_m_train)
        X_m_train = kpex.select_features(X_m_train, 'MULTI', is_fulltext)
        X_m_train, y_m_train = RandomOverSampler(random_state=65).fit_resample(X_m_train, y_m_train)
        X_m_test, y_m_test = kpex.split_feats_label(df_m_test)

    # Train models
    
    if kw_type != 'MULTI':
        model_s = train(X_s_train, y_s_train)
        joblib.dump(model_s, "krapivin_single_model.sav")
    if kw_type != 'SINGLE':
        model_m = train(X_m_train, y_m_train)
        joblib.dump(model_m, "krapivin_multi_model.sav")

    # Evaluate models

    eval_krapivin.evaluate_models(
        model_s if kw_type != 'MULTI' else None,
        model_m if kw_type != 'SINGLE' else None,
        X_s_test if kw_type != 'MULTI' else None,
        X_m_test if kw_type != 'SINGLE' else None,
        y_s_test if kw_type != 'MULTI' else None,
        y_m_test if kw_type != 'SINGLE' else None,
    )
    

def train(X, y):
    
    model = GradientBoostingClassifier(random_state=656)
    model.fit(X, y)

    return model


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="")
    parser.add_argument("TRAIN_TSV", help='Krapivin train TSV file')
    parser.add_argument("TEST_TSV", help='Krapivin test TSV file')
    parser.add_argument("--fulltext", action='store_true', default=False, help='Texts contain fulltexts')
    parser.add_argument("--type", required=False, default='ALL', help='ALL | SINGLE | MULTI')
    args = parser.parse_args()

    kw_type = args.type.upper()
    if kw_type not in ['ALL', 'SINGLE', 'MULTI']:
        raise ValueError("--type argument must be one of: ALL, SINGLE, MULTI")

    main(args.TRAIN_TSV, args.TEST_TSV, args.fulltext, kw_type)
