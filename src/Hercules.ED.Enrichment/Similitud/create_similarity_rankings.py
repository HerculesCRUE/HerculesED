import argparse
import os
import pdb

import pandas as pd

MIN_DF = 4
MAX_DF = 0.1


def main(train_fpath, test_fpath, n_similar, out_fpath, method):
    
    train = pd.read_csv(train_fpath, sep='\t')
    test = pd.read_csv(test_fpath, sep='\t')

    if method == 'TFIDF':
        sorted_indices = tfidf_similarity(train, test)
    else:
        sorted_indices = sbert_similarity(train, test)

    result_indices = sorted_indices[:, :n_similar]

    create_tsv(out_fpath, train, test, result_indices)


def tfidf_similarity(train, test):

    from sklearn.feature_extraction.text import TfidfVectorizer
    from sklearn.metrics.pairwise import cosine_similarity

    vectorizer = TfidfVectorizer(min_df=MIN_DF, max_df=MAX_DF)
    train_tfidf = vectorizer.fit_transform(train.text.values.astype('U'))
    test_tfidf = vectorizer.transform(test.text.values.astype('U'))

    similarity_matrix = cosine_similarity(test_tfidf, train_tfidf)
    sorted_indices = (-similarity_matrix).argsort()

    return sorted_indices


def sbert_similarity(train, test):
    
    from sentence_transformers import SentenceTransformer, util

    model = SentenceTransformer('all-MiniLM-L6-v2')

    train_embs = model.encode(train.text.values.astype('U'))
    test_embs = model.encode(test.text.values.astype('U'))
    
    cos_sim = util.cos_sim(test_embs, train_embs)
    sorted_indices = (-cos_sim).argsort()

    return sorted_indices.numpy()

    
def create_tsv(fpath, train, test, result_indices):

    data = {
        'ref_doc_id': [],
        'ref_text': [],
        'rank': [],
        'sim_doc_id': [],
        'sim_text': [],
    }

    for row_i in range(result_indices.shape[0]):
        for col_i in range(result_indices.shape[1]):
            test_idx = row_i
            train_idx = result_indices[row_i, col_i]
            test_row = test.iloc[test_idx]
            train_row = train.iloc[train_idx]
            rank = col_i + 1
            
            data['ref_doc_id'].append(test_row.id)
            data['ref_text'].append(test_row.text)
            data['rank'].append(rank)
            data['sim_doc_id'].append(train_row.id)
            data['sim_text'].append(train_row.text)

        data['ref_doc_id'].append("")
        data['ref_text'].append("")
        data['rank'].append("")
        data['sim_doc_id'].append("")
        data['sim_text'].append("")

    df = pd.DataFrame(data)
    df.to_csv(fpath, sep='\t', index=False)
    

if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="Find the N most similar documents for each document in the test")
    parser.add_argument("METHOD", choices=['TFIDF', 'SBERT'], help='Method to compute similarity: TFIDF or SBERT')
    parser.add_argument("TRAIN", help='TSV file of the train dataset (id, text)')
    parser.add_argument("TEST", help='TSV file of the test dataset (id, text)')
    parser.add_argument("N", type=int, help='Number of most similar items to return for each test document')
    parser.add_argument("OUT_TSV", help='TSV file with the result')
    args = parser.parse_args()

    main(args.TRAIN, args.TEST, args.N, args.OUT_TSV, args.METHOD)
