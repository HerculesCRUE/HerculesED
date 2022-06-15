import argparse
import os
import pdb

import pandas as pd

MIN_DF = 4
MAX_DF = 0.1

MODEL = 'all-MiniLM-L6-v2'
#MODEL = 'all-mpnet-base-v2'
# MODEL = 'paraphrase-multilingual-mpnet-base-v2'


def main(train_fpath, test_fpath, n_similar, out_fpath, method):
    
    train = pd.read_csv(train_fpath, sep='\t', dtype=str)
    test = pd.read_csv(test_fpath, sep='\t', dtype=str)

    if method == 'TFIDF':
        sorted_indices = tfidf_similarity(train, test)
    elif method == 'SBERT':
        sorted_indices = sbert_similarity(train, test)
    elif method == 'DESCRIPTORS':
        sorted_indices = descriptor_similarity(train, test, False)

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

    model = SentenceTransformer(MODEL)

    train_embs = model.encode(train.text.values.astype('U'))
    test_embs = model.encode(test.text.values.astype('U'))
    
    cos_sim = util.cos_sim(test_embs, train_embs)
    sorted_indices = (-cos_sim).argsort()

    return sorted_indices.numpy()


def descriptor_similarity(train, test, do_filter):

    from sklearn.feature_extraction import DictVectorizer
    from sklearn.metrics.pairwise import cosine_similarity

    def load_dataset(data):
        docs = []
        for i, row in data.iterrows():
            descriptors_ = row.keyphrase.split('|')
            if not do_filter:
                descriptors_.extend([ t for t in row.topic.split('|') if t != '' ])
            probs_ = [ float(p) for p in row.keyphrase_prob.split('|') ]
            if not do_filter:
                probs_.extend([ float(p) for p in row.topic_prob.split('|') if p != '' ])
            doc = {}
            for j in range(len(descriptors_)):
                doc[descriptors_[j]] = probs_[j]
            docs.append(doc)
        return docs

    # def filter_rankings_by_topic(indices, train, test):
    #     for row_i in range(indices.shape[0]):
    #         test_item = test.iloc[row_i]
    #         topics_test = set([ t for t in test_item.topic if t != '' ])
    #         for col_i in range(indices.shape[1]):
    #             train_item = train.iloc[indices[row_i, col_i]]
    #             topics_train = set([ t for t in train_item.topic if t != '' ])
    #             common_topics = set.intersection(topics_test, topics_train)
    #             if len(common_topics) == 0:
    #                 indices[row_i, col_i] = -1
    
    train.fillna('', inplace=True)
    test.fillna('', inplace=True)
    
    train_descriptors = load_dataset(train)
    test_descriptors = load_dataset(test)

    vectorizer = DictVectorizer()
    train_vec = vectorizer.fit_transform(train_descriptors)
    test_vec = vectorizer.transform(test_descriptors)

    similarity_matrix = cosine_similarity(test_vec, train_vec)
    sorted_indices = (-similarity_matrix).argsort()

    return sorted_indices

    
def create_tsv(fpath, train, test, result_indices):

    data = {
        'ref_doc_id': [],
        'ref_text': [],
        'ref_descriptors': [],
        'ref_descriptor_probs': [],
        'rank': [],
        'sim_doc_id': [],
        'sim_text': [],
        'sim_descriptors': [],
        'sim_descriptor_probs': [],
    }

    for row_i in range(result_indices.shape[0]):
        
        for col_i in range(result_indices.shape[1]):
            test_idx = row_i
            train_idx = result_indices[row_i, col_i]
            test_row = test.iloc[test_idx]
            train_row = train.iloc[train_idx]
            rank = col_i + 1

            descriptor_names_ref = [ t.lower() for t in test_row.keyphrase.split('|') if t != '' ]
            descriptor_probs_ref = [ t for t in test_row.keyphrase_prob.split('|') if t != '' ]
            #descriptors_ref = { n: p for n, p in zip(descriptor_names_ref, descriptor_probs_ref) }
            descriptor_names_sim = [ t.lower() for t in train_row.keyphrase.split('|') if t != '' ]
            descriptor_probs_sim = [ t for t in train_row.keyphrase_prob.split('|') if t != '' ]
            #descriptors_sim = { n: p for n, p in zip(descriptor_names_sim, descriptor_probs_sim) }

            #common_descriptors = find_common_descriptors(descriptors_ref, descriptors_sim)
            #common_descriptor_names = [ c[0] for c in common_descriptors ]
            #common_descriptor_probs = [ f"{c[1]:.3f}" for c in common_descriptors ]
                
            data['ref_doc_id'].append(test_row.id)
            data['ref_text'].append(test_row.text)
            data['ref_descriptors'].append(' | '.join(descriptor_names_ref))
            data['ref_descriptor_probs'].append(' | '.join(descriptor_probs_ref))
            data['rank'].append(rank)
            data['sim_doc_id'].append(train_row.id)
            data['sim_text'].append(train_row.text)
            data['sim_descriptors'].append(' | '.join(descriptor_names_sim))
            data['sim_descriptor_probs'].append(' | '.join(descriptor_probs_sim))
            #data['common_descriptors'].append(' | '.join(sorted(common_descriptor_names)))
            #data['common_descriptor_probs'].append(' | '.join(sorted(common_descriptor_probs)))

        data['ref_doc_id'].append("")
        data['ref_text'].append("")
        data['ref_descriptors'].append("")
        data['ref_descriptor_probs'].append("")
        data['rank'].append("")
        data['sim_doc_id'].append("")
        data['sim_text'].append("")
        data['sim_descriptors'].append("")
        data['sim_descriptor_probs'].append("")
        #data['common_descriptors'].append("")
        #data['common_descriptor_probs'].append("")

    df = pd.DataFrame(data)
    df.to_csv(fpath, sep='\t', index=False)


def find_common_descriptors(descriptors1, descriptors2):
    
    common = set()
    for desc, prob1 in descriptors1.items():
        if desc in descriptors2:
            prob2 = descriptors2[desc]
            common.add( (desc, prob1*prob2) )
    for desc, prob2 in descriptors2.items():
        if desc in descriptors1:
            prob1 = descriptors1[desc]
            common.add( (desc, prob1*prob2) )

    common = sorted(list(common), key=lambda x: x[1], reverse=True)
    common = [ c for c in common[:5] ]

    return common
    

if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="Find the N most similar documents for each document in the test")
    parser.add_argument("METHOD", choices=['TFIDF', 'SBERT', 'DESCRIPTORS'], help='Method to compute similarity: TFIDF | SBERT | DESCRIPTORS')
    parser.add_argument("TRAIN", help='TSV file of the train dataset (id, text [, keyphrase, prob])')
    parser.add_argument("TEST", help='TSV file of the test dataset (id, text [, keyphrase, prob])')
    parser.add_argument("N", type=int, help='Number of most similar items to return for each test document')
    parser.add_argument("OUT_TSV", help='TSV file with the result')
    args = parser.parse_args()

    main(args.TRAIN, args.TEST, args.N, args.OUT_TSV, args.METHOD)
