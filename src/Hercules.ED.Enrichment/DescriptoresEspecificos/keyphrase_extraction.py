import pickle, joblib
import pandas as pd
import numpy as np
import spacy
import math
import re
import pdb

SPACY_MODEL = 'en_core_web_lg'

KW_MAX_LEN = 50
KW_MIN_LEN = 3

# generic KWs will be removed if they are part of any of the first N_REFERENCE_KWS kws
N_REFERENCE_KWS = 15

SINGLE_W_ATTRS_FULLTEXT = ['LENGTH', 'IN_TITLE?', 'IN_ABSTRACT?', 'OFFSET',
                           'IDF_CLEF', 'TFIDF_CLEF', 'LLR_CLEF', 'LLR_SCOPUS']
SINGLE_W_ATTRS_SHORT = ['LENGTH', 'IN_TITLE?', 'IN_ABSTRACT?', 'OFFSET',
                        'IDF_CLEF', 'TFIDF_CLEF', 'LLR_CLEF', 'LLR_SCOPUS']
MULTI_W_ATTRS_FULLTEXT = ['LENGTH', 'IN_TITLE?', 'IN_ABSTRACT?', 'OFFSET', 'NORM_OFFSET',
                          'SPREAD', 'NESTED_RATE', 'LLR_CLEF']
MULTI_W_ATTRS_SHORT = ['LENGTH', 'OFFSET', 'SPREAD', 'NESTED_RATE', 'LLR_CLEF']


def clean_dataset(ds):
    
    ds = ds[~ds.isin([np.nan, np.inf, -np.inf]).any(1)]
    ds = ds.dropna()
    ds = ds.reset_index(drop=True)

    ds['KW'] = ds['KW'].astype(str)
    ds['IN_TITLE?'] = ds['IN_TITLE?'].astype(int)
    ds['IN_ABSTRACT?'] = ds['IN_ABSTRACT?'].astype(int)
    
    return ds


def split_single_multi(ds):

    ds_s = ds[~ds.KW.str.contains(' |-', regex=True)]
    ds_m = ds[ds.KW.str.contains(' |-', regex=True)]

    return ds_s, ds_m


def split_feats_label(ds):

    X = ds.drop(['IS_KEYWORD?'], axis=1)
    y = ds['IS_KEYWORD?']

    return X, y


def select_features(X, kw_type, contains_fulltext):

    assert kw_type in ['SINGLE', 'MULTI']
    assert type(contains_fulltext) == bool

    feats = []
    if contains_fulltext:
        if kw_type == 'SINGLE':
            feats = SINGLE_W_ATTRS_FULLTEXT
        else:  # MULTI
            feats = MULTI_W_ATTRS_FULLTEXT
    else:
        if kw_type == 'SINGLE':
            feats = SINGLE_W_ATTRS_SHORT
        else:  # MULTI
            feats = MULTI_W_ATTRS_SHORT
            
    return X[feats]


def select_features_by_model(X, model):

    feats = model.feature_names_in_
            
    return X[feats]


def remove_similar_kws(data):

    seen_extended = set()
    indices_to_drop = []
    for i in data.index:
        kw = data.at[i, 'KW'].lower()
        # check if needs to be removed
        if kw in seen_extended:
            indices_to_drop.append(i)
        # generate similar
        seen_extended.add(kw)
        if len(kw) > 2:
            seen_extended.add(kw[:-1])
        seen_extended.add(kw+'s')

    data = data.drop(index=indices_to_drop)
        
    return data


def remove_generic_kws(data):

    def _generate_subkws(kw):
        subkws = set()
        tokens = kw.split()
        if len(tokens) > 1:
            for i in range(0, len(tokens)):
                subkws.add(tokens[i].lower())
                subkws.add(tokens[i][:-1].lower())
                subkws.add((tokens[i]+'s').lower())
        return subkws

    ref_kws = set(data.head(N_REFERENCE_KWS).KW.to_numpy())
    generic_forms = set.union(*[ _generate_subkws(kw) for kw in ref_kws])
    
    for i, row in data.iterrows():
        if row.KW.lower() in generic_forms:
            data.at[i, 'PROB'] = 0.0

    data = data.sort_values('PROB', ascending=False)
    
    return data


class KeyphraseExtractor:
    
    def __init__(self, model_s_fpath, model_m_fpath, clef_fpath, scopus_fpath, clef_idf_fpath):
        
        self._model_s = joblib.load(model_s_fpath)
        self._model_m = joblib.load(model_m_fpath)

        self._feature_extractor = FeatureExtractor(clef_fpath, scopus_fpath, clef_idf_fpath)

        
    def extract_keyphrases(self, title, abstract, body, return_n=10):

        X = self._feature_extractor.extract_features(title, abstract, body)
            
        ranking = self.create_ranking(self._model_s, self._model_m, X)
        ranking = ranking.head(return_n)

        return list(zip(ranking.KW, ranking.PROB))


    # If the y parameter is set, the returning dataframe contains IS_KEYWORD? column
    @staticmethod
    def create_ranking(model_s, model_m, X, y=None):

        if y is not None:
            assert len(X) == len(y)
            X['IS_KEYWORD?'] = y

        X = clean_dataset(X)
        X_s, X_m = split_single_multi(X)

        if y is not None:
            X_s, y_s = split_feats_label(X_s)
            X_m, y_m = split_feats_label(X_m)
            y = np.concatenate([y_s.to_numpy(), y_m.to_numpy()])
            
        kws = np.concatenate([X_s.KW.to_numpy(), X_m.KW.to_numpy()])

        if model_s is not None:
            X_s = select_features_by_model(X_s, model_s)
        if model_m is not None:
            X_m = select_features_by_model(X_m, model_m)

        y_pred_prob_s, y_pred_prob_m = [], []
        if len(X_s) > 0:
            y_pred_prob_s = model_s.predict_proba(X_s)[:,1]
        if len(X_m) > 0:
            y_pred_prob_m = model_m.predict_proba(X_m)[:,1]

        y_pred_prob = np.concatenate([y_pred_prob_s, y_pred_prob_m])

        ranking_data = {
            'KW': kws,
            'PROB': y_pred_prob
        }
        if y is not None:
            ranking_data['IS_KEYWORD?'] = y
            
        ranking = pd.DataFrame(ranking_data)
        ranking = ranking.sort_values('PROB', ascending=False)
        ranking = remove_similar_kws(ranking)
        ranking = remove_generic_kws(ranking)

        return ranking


class FeatureExtractor:
    
    VALID_DEP_RELS = set(['advmod', 'amod', 'case', 'cc', 'compound', 'conj', 'mark',
                          'neg', 'nmod', 'npadvmod', 'poss', 'punct', 'quantmod', 'root'])

    CLEF_SIZE = 90e6
    SCOPUS_SIZE = 146e6
    
    def __init__(self, clef_fpath, scopus_fpath, clef_idf_fpath):

        self._nlp = spacy.load(SPACY_MODEL)
        
        with open(clef_fpath, 'rb') as f:
            self._clef = pickle.load(f)
        with open(scopus_fpath, 'rb') as f:
            self._scopus = pickle.load(f)
        self._idf_clef = joblib.load(clef_idf_fpath)
        
        
    def extract_features(self, title, abstract, body, remove_similar=False):

        fulltext = title + ' ' + abstract + ' ' + body
        fulltext_rev = fulltext[::-1]
        fulltext_len = len(fulltext)

        title_kw_cands, title_phrases, title_doc = self._extract_keyword_candidates(title)
        abstract_kw_cands, abstract_phrases, abstract_doc = self._extract_keyword_candidates(abstract)
        body_kw_cands, body_phrases, _ = self._extract_keyword_candidates(body)
        all_kw_cands = title_kw_cands + abstract_kw_cands + body_kw_cands
        all_phrases = title_phrases + abstract_phrases + body_phrases

        title_kw_cands_str = set([ kwc[1].lower() for kwc in title_kw_cands ])
        abstract_kw_cands_str = set([ kwc[1].lower() for kwc in abstract_kw_cands ])
        all_kw_cands_str = [ kwc[1].lower() for kwc in all_kw_cands ]
        
        unique_kw_cands = {}
        for kwc_span, kwc, lemma in all_kw_cands:
            kwcl = kwc.lower()
            if kwcl == kwc or (kwcl not in unique_kw_cands):
                unique_kw_cands[kwcl] = (kwc_span, kwc, lemma)
        unique_kw_cands = list(unique_kw_cands.values())
        kw_cand_freqs = { form.lower(): all_kw_cands_str.count(form.lower()) for span, form, lemma in unique_kw_cands }

        features = {}
        for kwc_span, kwc, kwc_lemma in unique_kw_cands:
            length = len(kwc)
            in_title = kwc.lower() in title_kw_cands_str
            in_abstract = kwc.lower() in abstract_kw_cands_str
            kwc_match = re.search(r"\b"+kwc+r"\b", fulltext, re.IGNORECASE)
            offset = kwc_match.start() if kwc_match else fulltext_len
            kwc_match = re.search(r"\b"+kwc[::-1]+r"\b", fulltext_rev, re.IGNORECASE)
            last_offset = fulltext_len - kwc_match.end() if kwc_match else fulltext_len
            spread = last_offset - offset
            norm_offset = offset / len(fulltext)
            nested_rate = self._get_nested_rate(kwc, all_phrases)
            kwc_idf_clef = self._idf_clef[kwc] if kwc in self._idf_clef else 0.0
            clef_freq = self._clef['freqs'][kwc_lemma] if kwc_lemma in self._clef['freqs'] else 0
            scopus_freq = self._scopus['freqs'][kwc_lemma] if kwc_lemma in self._scopus['freqs'] else 0
            doc_len = len(fulltext.split())
            article_freq = kw_cand_freqs[kwc.lower()]
            tfidf_clef = FeatureExtractor._tf_idf(article_freq, doc_len, clef_freq, self.CLEF_SIZE)
            llr_clef = FeatureExtractor._association_measure(article_freq, clef_freq, doc_len, self.CLEF_SIZE)
            llr_scopus = FeatureExtractor._association_measure(article_freq, scopus_freq, doc_len, self.SCOPUS_SIZE)

            self._add_features(features, {
                'KW': kwc,
                'LENGTH': length,
                'IN_TITLE?': in_title,
                'IN_ABSTRACT?': in_abstract,
                'OFFSET': offset,
                'NORM_OFFSET': norm_offset,
                'SPREAD': spread,
                'NESTED_RATE': nested_rate,
                'IDF_CLEF': kwc_idf_clef,
                'TFIDF_CLEF': tfidf_clef,
                'LLR_CLEF': llr_clef,
                'LLR_SCOPUS': llr_scopus,
            })

        df = pd.DataFrame(features)

        df = df.sort_values('OFFSET')
        if remove_similar:
            df = remove_similar_kws(df)
        df = df.reset_index(drop=True)

        return df


    def _extract_keyword_candidates(self, text):

        doc = self._nlp(text)

        kw_cands = []
        phrases = []  # keep track of all noun phrases, later used to compute nested_rate
        for chunk in doc.noun_chunks:
            undividable_chunks = self._get_undividable_chunks(chunk.root)
            undividable_chunks = self._join_spans(undividable_chunks)

            chunk_kw_cands = []
            main_chunk, _ = undividable_chunks[-1]
            for i in reversed(range(len(undividable_chunks))):
                undividable_chunk, rel = undividable_chunks[i]
                if rel not in self.VALID_DEP_RELS:
                    break
                kw_span = doc[undividable_chunk.start : main_chunk.end]
                form = self._span_to_form(kw_span)
                if len(form) > KW_MIN_LEN and len(form) < KW_MAX_LEN and form[0] != '-' and form[-1] != '-':
                    lemma = self._span_to_lemma(kw_span)
                    chunk_kw_cands.append( (kw_span, form, lemma) )
                    
            kw_cands.extend(chunk_kw_cands)
            if len(chunk_kw_cands) > 0:
                phrases.append(max(chunk_kw_cands, key=lambda kwc: len(kwc[1])))

        return kw_cands, phrases, doc


    @classmethod
    def _span_to_form(cls, span):
        
        text = cls._normalize_case(span)
        text = cls._clean_kw(text)

        return text

    
    @classmethod
    def _span_to_lemma(cls, span):
        
        lemma = ' '.join([ w.lemma_.lower() for w in span if w.text != '-' ])
        lemma = cls._clean_kw(lemma)
        lemma = '_'.join(lemma.split())

        return lemma

    
    @staticmethod
    def _join_spans(chunks):

        new_chunks = []
        for i, (chunk, rel) in enumerate(chunks):
            if i > 0 and (chunk.text[0] == '-' or
                          chunks[i-1][0].text[-1] == '-' or
                          chunk.text[0] == ')' or
                          chunks[i-1][0].text[-1] == '('):
                prev_chunk, prev_rel = new_chunks.pop()
                new_chunk = chunk.doc[prev_chunk.start:chunk.end]
                new_chunks.append( (new_chunk, rel) )
            else:
                new_chunks.append( (chunk, rel) )

        return new_chunks


    @classmethod
    def _get_undividable_chunks(cls, root):

        chunks = []
        
        main_chunk = ([root.i, root.i+1], 'root')  # main chunk (containing the root)
        chunks.append(main_chunk)
        main_chunk_done = False
        
        for rel_word in reversed(list(root.lefts)):
            if not main_chunk_done:
                if rel_word.dep_ in ['compound', 'punct']:  # while main chunk
                    rel_chunk = cls._get_undividable_chunk(rel_word)
                    main_chunk[0][0] = rel_chunk[0][0]
                else:
                    main_chunk_done = True
            if main_chunk_done:  # cannot be replaced by "else"!
                if rel_word.dep_ in cls.VALID_DEP_RELS:
                    chunks.insert(0, cls._get_undividable_chunk(rel_word))
                else:
                    break

        return [ (root.doc[ch[0]:ch[1]], rel) for ch, rel in chunks ]


    @classmethod
    def _get_undividable_chunk(cls, word):

        chunk = ([word.i, word.i+1], word.dep_)

        for rel_word in reversed(list(word.lefts)):
            if rel_word.dep_ in cls.VALID_DEP_RELS:
                rel_word_chunk = cls._get_undividable_chunk(rel_word)
                chunk[0][0] = rel_word_chunk[0][0]
            else:
                break

        for rel_word in word.rights:
            if rel_word.dep_ in cls.VALID_DEP_RELS:
                rel_word_chunk = cls._get_undividable_chunk(rel_word)
                chunk[0][1] = rel_word_chunk[0][1]
            else:
                break

        return chunk
            

    @staticmethod
    def _clean_kw(kw):

        return ''.join(ch for ch in kw if ch.isalnum() or ch in " -").strip()

    
    @staticmethod
    def _normalize_case(spacy_span):

        if all([ w.ent_iob_ == 'O' for w in spacy_span ]):
            return spacy_span.text.lower()
        else:
            return spacy_span.text

        
    @classmethod
    def _get_nested_rate(cls, term, phrases):

        term = term.lower()

        n_term = len([ p for p in phrases if p[1].lower() == term ])
        n_nested = len([ p for p in phrases
                         if re.search(r"\b"+term+r"\b", p[1], re.IGNORECASE) is not None
                         and p[1].lower() != term ])

        if n_nested + n_term == 0:
            return 1.0

        return n_nested / (n_nested + n_term)

    
    @staticmethod
    def _tf_idf(doc_freq, len_doc, corp_freq, word_tot):

        if corp_freq == 0:
            corp_freq = 1
        tf = float(doc_freq)/float(len_doc)
        idf = math.log(float(word_tot)/float(corp_freq))

        return tf * idf


    @staticmethod
    def _association_measure(n11, n1p, np1, npp):

        n1p = n1p+1
        np2 = npp-np1
        n2p = npp-n1p
        n12 = n1p-n11
        if n12 <= 0:
            n12 = 1
        n21 = np1-n11
        n22 = n2p-n21

        m11 = float(np1*n1p)/npp
        m12 = float(np2*n1p)/npp
        m21 = float(np1*n2p)/npp
        m22 = float(np2*n2p)/npp

        if n11 == 0:
            n11divm11 = 1
        else:
            n11divm11 = n11/m11
        if n12 == 0:
            n12divm12 = 1
        else:
            n12divm12 = n12/m12
        if n21 == 0:
            n21divm21 = 1
        else:
            n21divm21 = n21/m21
        if n22 == 0:
            n22divm22 = 1
        else:
            n22divm22 = n22/m22
        log_likelihood = 2 * (n11 * math.log(n11divm11) + n12 * math.log(
            n12divm12) + n21 * math.log(n21divm21) + n22 * math.log(n22divm22))

        return log_likelihood


    @staticmethod
    def _add_features(data, feats):

        for k, v in feats.items():
            if k not in data:
                data[k] = []
            data[k].append(v)

