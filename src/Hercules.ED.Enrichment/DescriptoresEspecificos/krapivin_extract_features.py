import argparse
import pickle
import joblib
import spacy
import resource
import math
import os
import sys
import re
import pdb

import pandas as pd

from keyphrase_extraction import FeatureExtractor


def main(krapivin_dir, clef_fpath, scopus_fpath, clef_idf_fpath, out_fpath, use_fulltext):

    fex = FeatureExtractor(clef_fpath, scopus_fpath, clef_idf_fpath)

    doc_feats = []
    
    txt_fnames = [ f for f in os.listdir(krapivin_dir) if os.path.splitext(f)[1] == '.txt' ]
    for i, txt_fname in enumerate(txt_fnames):

        txt_fpath = os.path.join(krapivin_dir, txt_fname)
        key_fpath = os.path.splitext(txt_fpath)[0] + '.key'

        title, abstract, body = split_sections(txt_fpath)
        body = body if use_fulltext else ""
        fulltext = title + ' ' + abstract + ' ' + body            
        gold_kws = extract_true_keywords(key_fpath, fulltext)

        feats = fex.extract_features(title, abstract, body)
        docids = feats.KW.map(lambda kw: txt_fname)
        feats.insert(0, 'DOCID', docids)
        feats['IS_KEYWORD?'] = feats.KW.map(lambda kw: is_true_kw(kw, gold_kws))
        doc_feats.append(feats)
        
        print(f"Progress: {i+1}/{len(txt_fnames)}", end='\r')

    df = pd.concat(doc_feats)
    df.to_csv(out_fpath, sep='\t', index=False)
            

def split_sections(fpath):

    with open(fpath, 'r') as f:
        text = f.read()
        title = text[text.find('--T')+3:text.find('\n--A\n')].strip()
        abstract = text[text.find('\n--A\n')+4:text.find('\n--B\n')].strip()
        body = text[text.find('\n--B\n')+4:text.find('\n--R\n')].strip()
        
    return (' '.join(title.split()), ' '.join(abstract.split()), ' '.join(body.split()))


def extract_true_keywords(fpath, text):

    text = text.lower()
    
    kws = set()
    with open(fpath, 'r') as f:
        for ln in f:
            kw = ln.strip()
            if kw[-1] == '.' or kw[-1] == '?':
                kw = kw[:-1]
            kw = clean_kw(kw).lower()
            pattern = kw[:-1] + r"(" + kw[-1] + r"s?)?"
            #pattern = kw
            if re.search(r"\b"+pattern+r"\b", text) is not None:
                kw = kw.replace('-', ' ')
                kws.add(kw)
                kws.add(kw[:-1])
                kws.add(kw+'s')
            
    return kws


def clean_kw(kw):

    return ''.join(ch for ch in kw if ch.isalnum() or ch in " -").strip()

                
def is_true_kw(kw, gold_kws):

    kw = kw.lower().replace('-', ' ')

    return kw in gold_kws


if __name__ == '__main__':

    parser = argparse.ArgumentParser()
    parser.add_argument("KRAPIVIN_DIR", help='Path containing Krapivin dataset documents')
    parser.add_argument("CLEF", help='Structure containing term counts in CLEF (clef.pkl)')
    parser.add_argument("SCOPUS", help='Structure containing term counts in Scopus (scopus.pkl)')
    parser.add_argument("CLEF_IDF", help='Structure containing IDF counts in CLEF (idfakCLEF.pkl)')
    parser.add_argument("OUT_TSV", help='Path of the TSV file to be created')
    parser.add_argument("--fulltext", action='store_true', default=False, help='Extract candidates from the full text, not only from title+abstract')
    args = parser.parse_args()
    
    main(args.KRAPIVIN_DIR, args.CLEF, args.SCOPUS, args.CLEF_IDF, args.OUT_TSV, args.fulltext)

