import argparse
import sys
import csv
import pdb

from nltk.tokenize import RegexpTokenizer

from sklearn.preprocessing import MultiLabelBinarizer
from sklearn.feature_extraction.text import TfidfVectorizer,CountVectorizer
from sklearn.neighbors import KNeighborsClassifier
from sklearn.ensemble import GradientBoostingClassifier
from sklearn.svm import LinearSVC
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import classification_report

from imblearn.over_sampling import RandomOverSampler

ALGORITHMS = ['svm', 'lr']


def main(train_fpath, test_fpath, algorithm, lemmatize, debug, max_iter):

    if algorithm not in ALGORITHMS:
        raise ValueError(f"Algorithm value must be one of {'|'.join(ALGORITHMS)}")

    X_train, y_train, orig_texts_train = load_tsv(train_fpath, lemmatize)
    X_test, y_test, orig_texts_test = load_tsv(test_fpath, lemmatize)
    y = y_train + y_test
    print("Datasets loaded", file=sys.stderr)
    
    multilabel_binarizer = MultiLabelBinarizer()
    multilabel_binarizer.fit(y)
    y_train = multilabel_binarizer.transform(y_train)
    y_test = multilabel_binarizer.transform(y_test)
    print("Multilabel binarization done", file=sys.stderr)

    vectorizer = TfidfVectorizer(min_df=4, max_df=0.1, norm="l2")
    vecfit = vectorizer.fit(X_train)
    X_train_tfidf = vecfit.transform(X_train)
    X_test_tfidf = vecfit.transform(X_test)
    print("Tfidf done", file=sys.stderr)

    for i in range(y_train.shape[1]):

        category = multilabel_binarizer.classes_[i]

        print(f"-------------------------------------------------------------------", file=sys.stderr)
        print(f"Training classifier for '{category}'", file=sys.stderr)
        print(f"-------------------------------------------------------------------", file=sys.stderr)
        
        y_train_cat = y_train[:,i]
        y_test_cat = y_test[:,i]
        ros = RandomOverSampler()

        X_resampled, y_resampled = ros.fit_resample(X_train_tfidf, y_train_cat)

        if algorithm == 'lr':
            if max_iter is None:
                lr = LogisticRegression()
            else:
                lr = LogisticRegression(max_iter=max_iter)
        elif algorithm == 'svm':
            if max_iter is None:
                lr = LinearSVC()
            else:
                lr = LinearSVC(max_iter=max_iter)
        #lr = GradientBoostingClassifier()
        #lr = KNeighborsClassifier()
            
        lr.fit(X_resampled, y_resampled)
        k_pred = lr.predict(X_test_tfidf)

        result = classification_report(y_test_cat, k_pred, output_dict=True)
        print('\t'.join([ category, str(result['1']['precision']), str(result['1']['recall']), str(result['1']['f1-score']), str(result['1']['support']) ]))        

        if debug:
            for (pred, ref, text) in zip(k_pred, y_test_cat, X_test):
                if pred == 1:
                    print(f"CATEGORY: {category}", file=sys.stderr)
                    print(f"PREDICTION: {pred}", file=sys.stderr)
                    print(f"TRUE LABEL: {ref}", file=sys.stderr)
                    print(f"TEXT: {text}", file=sys.stderr)
                    print(file=sys.stderr)
    

def load_tsv(fpath, lemmatize):

    tokenizer = RegexpTokenizer(r'\w+')
    
    orig_texts = []
    texts = []
    categories = []

    with open(fpath, 'r') as f:
        reader = csv.DictReader(f, delimiter='\t')
        if reader.fieldnames[:2] != ['id', 'text']:
            raise ValueError("Input TSV headers must be [id, text, {cat1}, {cat2}, ..., {catN}]")
        category_ids = reader.fieldnames[2:]
        for row in reader:
            row_cats = set([ cat for cat in category_ids if row[cat] == '1' ])
            text = row['text'].lower()
            if lemmatize:
                # lemmatize!
                lemmas = tokenizer.tokenize(text)
                text = ' '.join(lemmas)
            else:
                tokens = tokenizer.tokenize(text)
                text = ' '.join(tokens)
            text = text.lower()

            orig_texts.append(row['text'])
            texts.append(text)
            categories.append(row_cats)

    return texts, categories, orig_texts


if __name__ == '__main__':
    
    parser = argparse.ArgumentParser(description="Trains and tests a model for multilabel text classification using LR or SVM algorithms.")
    parser.add_argument("TRAIN", help='Train set in TSV format (id, text, [labels])')
    parser.add_argument("TEST", help='Test set in TSV format (id, text, [labels])')
    parser.add_argument("ALGORITHM", help='LR | SVM')
    parser.add_argument("-l", "--lemmatize", action="store_true", default=False, help='Use lemmas instead of tokens')
    parser.add_argument("-d", "--debug", action="store_true", default=False, help='Print to the standard output texts predicted as true for each category')
    parser.add_argument("--max-iter", required=False, type=int, default=False, help='max_iter argument of LogisticRegression or SVC classes')
    args = parser.parse_args()

    main(args.TRAIN, args.TEST, args.ALGORITHM.lower(), args.lemmatize, args.debug, args.max_iter)
