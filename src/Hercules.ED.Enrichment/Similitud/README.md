# Setting up the environment

Create and load a new Python3 virtual environment:

```
virtualenv -p python3 venv
source venv/bin/activate
```

Install the dependencies:

```
pip install -r requirements.txt
```

Download the datasets from the following URL:

```
wget https://storage.googleapis.com/elhuyar/Hercules/semantic_similarity/papers.train.tsv.bz2
wget https://storage.googleapis.com/elhuyar/Hercules/semantic_similarity/papers.test.tsv.bz2
```

# Retrieving similar papers

To get the 100 most similar papers of each paper in the test-set among the papers in the train-set:

```
python3 create_similarity_rankings.py TFIDF papers.train.tsv papers.test.tsv 100 result.tfidf.tsv
python3 create_similarity_rankings.py SBERT papers.train.tsv papers.test.tsv 100 result.sbert.tsv
```
