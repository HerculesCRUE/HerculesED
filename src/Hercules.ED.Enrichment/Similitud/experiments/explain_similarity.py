import argparse
import math
import os
import pdb

import pandas as pd
import nltk

SIMILARITY_THRESHOLD_SBERT = 0.0
SIMILARITY_THRESHOLD_WEIGHTED_SBERT = 0.0
SIMILARITY_THRESHOLD_SBERT_ACC = 0.0
SIMILARITY_THRESHOLD_OVERLAP = 0.0
MAX_KEYPHRASES = 5
MODEL = 'all-MiniLM-L6-v2'
# MODEL = 'all-mpnet-base-v2'
# MODEL = 'paraphrase-multilingual-mpnet-base-v2'


def main(method, in_fpath, out_fpath):

    if method in ['WEIGHTED_SBERT', 'SBERT', 'SBERT_ACC']:
        from sentence_transformers import SentenceTransformer
        model = SentenceTransformer(MODEL)
        if method == 'WEIGHTED_SBERT':
            SIMILARITY_THRESHOLD = SIMILARITY_THRESHOLD_WEIGHTED_SBERT
        elif method == 'SBERT':
            SIMILARITY_THRESHOLD = SIMILARITY_THRESHOLD_SBERT
        elif method == 'SBERT_ACC':
            SIMILARITY_THRESHOLD = SIMILARITY_THRESHOLD_SBERT_ACC
    elif method == 'OVERLAP':
        SIMILARITY_THRESHOLD = SIMILARITY_THRESHOLD_OVERLAP
    else:
        raise Exception("UNKNOWN METHOD")
        
    data = pd.read_csv(in_fpath, sep='\t', dtype=str)
    similarity_keyphrases = []
    similarity_keyphrase_probs = []

    for i, row in data.iterrows():
        if type(row.ref_text) == float and math.isnan(row.ref_text):
            similarity_keyphrases.append(None)
            similarity_keyphrase_probs.append(None)
            continue

        if pd.isnull(row.ref_descriptors):
            keyphrases_ref = {}
        else:
            keyphrases_ref = { n: float(p) for n, p in zip(row.ref_descriptors.split(' | '),
                                                           row.ref_descriptor_probs.split(' | ')) }
        if pd.isnull(row.sim_descriptors):
            keyphrases_sim = {}
        else:
            keyphrases_sim = { n: float(p) for n, p in zip(row.sim_descriptors.split(' | '),
                                                           row.sim_descriptor_probs.split(' | ')) }

        if method == 'SBERT':
            result_keyphrases = keyphrases_by_sbert(keyphrases_ref, keyphrases_sim, row.ref_text, row.sim_text, model)
        elif method == 'WEIGHTED_SBERT':
            result_keyphrases = keyphrases_by_weighted_sbert(keyphrases_ref, keyphrases_sim, row.ref_text, row.sim_text, model)
        elif method == 'SBERT_ACC':
            result_keyphrases = keyphrases_by_sbert_acc(keyphrases_ref, keyphrases_sim, row.ref_text, row.sim_text, model)
        elif method == 'OVERLAP':
            result_keyphrases = keyphrases_by_overlapping(keyphrases_ref, keyphrases_sim)
        else:
            raise Exception("UNKNOWN METHOD")

        # result_keyphrases: [ (keyphrase, score), (keyphrase, score), ...]
        result_keyphrases.sort(key=lambda x: x[1], reverse=True)
        result_keyphrases = [ k for k in result_keyphrases if k[1] > SIMILARITY_THRESHOLD ]
        result_keyphrases = remove_similar(result_keyphrases)
        result_keyphrases = result_keyphrases[:MAX_KEYPHRASES]
        result_keyphrase_names = [ k[0] for k in result_keyphrases ]
        result_keyphrase_probs = [ f"{k[1]:.3f}" for k in result_keyphrases ]
        similarity_keyphrases.append(' | '.join(result_keyphrase_names))
        similarity_keyphrase_probs.append(' | '.join(result_keyphrase_probs))

    data['similarity_keyphrases'] = similarity_keyphrases
    data['similarity_keyphrase_probs'] = similarity_keyphrase_probs

    data.to_csv(out_fpath, sep='\t', index=False)


def keyphrases_by_overlapping(keyphrases_ref, keyphrases_sim):

    normalize = lambda n: (n if n[-1] != 's' else n[:-1]).replace('-', ' ')
    norm_keyphrases_ref = { normalize(n): p for n, p in keyphrases_ref.items() }
    norm_keyphrases_sim = { normalize(n): p for n, p in keyphrases_sim.items() }
    
    common = set()
    for desc, prob1 in norm_keyphrases_ref.items():
        if desc in norm_keyphrases_sim:
            prob2 = norm_keyphrases_sim[desc]
            common.add( (desc, prob1*prob2) )
    for desc, prob2 in norm_keyphrases_sim.items():
        if desc in norm_keyphrases_ref:
            prob1 = norm_keyphrases_ref[desc]
            common.add( (desc, prob1*prob2) )

    common = sorted(list(common), key=lambda x: x[1], reverse=True)
    common = [ c for c in common[:5] ]

    return common


def keyphrases_by_sbert(keyphrases_ref, keyphrases_sim, ref_text, sim_text, model):

    ref_text_emb = model.encode(ref_text)
    sim_text_emb = model.encode(sim_text)
    text_emb = (ref_text_emb + sim_text_emb) / 2
    keyphrase_names_ref = list(keyphrases_ref.keys())
    keyphrase_names_sim = list(keyphrases_sim.keys())
    result_keyphrases_ref = get_keyphrase_score(keyphrase_names_ref, text_emb, model)
    result_keyphrases_ref = [ (k, d) for k, d in result_keyphrases_ref.items() ]
    result_keyphrases_sim = get_keyphrase_score(keyphrase_names_sim, text_emb, model)
    result_keyphrases_sim = [ (k, d) for k, d in result_keyphrases_sim.items() ]
    result_keyphrases = result_keyphrases_ref + result_keyphrases_sim

    return result_keyphrases


def keyphrases_by_weighted_sbert(keyphrases_ref, keyphrases_sim, ref_text, sim_text, model):

    ref_text_emb = model.encode(ref_text)
    sim_text_emb = model.encode(sim_text)
    keyphrase_names_ref = list(keyphrases_ref.keys())
    keyphrase_names_sim = list(keyphrases_sim.keys())
    result_keyphrases_ref = get_keyphrase_score(keyphrase_names_ref, sim_text_emb, model)
    result_keyphrases_ref = [ (k, d*keyphrases_ref[k]) for k, d in result_keyphrases_ref.items() ]
    result_keyphrases_sim = get_keyphrase_score(keyphrase_names_sim, ref_text_emb, model)
    result_keyphrases_sim = [ (k, d*keyphrases_sim[k]) for k, d in result_keyphrases_sim.items() ]
    result_keyphrases = result_keyphrases_ref + result_keyphrases_sim

    return result_keyphrases


def keyphrases_by_sbert_acc(keyphrases_ref, keyphrases_sim, ref_text, sim_text, model):

    ref_text_emb = model.encode(ref_text)
    sim_text_emb = model.encode(sim_text)
    keyphrase_names_ref = list(keyphrases_ref.keys())
    keyphrase_names_sim = list(keyphrases_sim.keys())
    final_score = lambda dist_self, dist_other: dist_self * dist_other**2
    distances_ref_ref = get_keyphrase_score(keyphrase_names_ref, ref_text_emb, model)
    distances_sim_ref = get_keyphrase_score(keyphrase_names_sim, ref_text_emb, model)
    distances_ref_sim = get_keyphrase_score(keyphrase_names_ref, sim_text_emb, model)
    distances_sim_sim = get_keyphrase_score(keyphrase_names_sim, sim_text_emb, model)
    distances_ref = [ (k, final_score(distances_ref_ref[k], distances_ref_sim[k])) for k in keyphrase_names_ref ]
    distances_sim = [ (k, final_score(distances_sim_sim[k], distances_sim_ref[k])) for k in keyphrase_names_sim ]
    result_keyphrases = distances_ref + distances_sim

    return result_keyphrases


def get_keyphrase_score(keyphrases, text_emb, model):

    from sentence_transformers import util

    if len(keyphrases) == 0:
        return {}
    
    keyphrases_emb = model.encode(keyphrases)
    cos_sim = util.cos_sim(keyphrases_emb, text_emb)
    cos_sim = cos_sim.squeeze().tolist()
    
    return { k: c for k, c in zip(keyphrases, cos_sim) }


def remove_similar(keyphrases):

    def are_similar(toks1, toks2):
        equal_toks = sum([ 1 for tok in toks1 if tok in toks2 ])
        equal_toks += sum([ 1 for tok in toks2 if tok in toks1 ])
        total_toks = len(toks1) + len(toks2)
        eq_rate = equal_toks / total_toks
        return eq_rate > 0.5

    similar_keyphrases = set()
    
    key_tokens = [ [ w if w[-1] != 's' else w[:-1] for w in keyphrase[0].lower().split() ]
                   for keyphrase in keyphrases ]
    for i in range(len(keyphrases)):
        for j in range(0, i):
            if j in similar_keyphrases:
                continue
            if are_similar(key_tokens[i], key_tokens[j]):
                similar_keyphrases.add(i)

    return [ key for i, key in enumerate(keyphrases) if i not in similar_keyphrases ]


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description="Explains how two articles are semantically similar.")
    parser.add_argument("METHOD", help="OVERLAP | WEIGHTED_SBERT | SBERT | SBERT_ACC")
    parser.add_argument("TSV", help="Ranking TSV of similar documents. It's the result of create_similarity_rankings.py")
    parser.add_argument("OUT_TSV", help='TSV file with the result')
    args = parser.parse_args()

    main(args.METHOD, args.TSV, args.OUT_TSV)
