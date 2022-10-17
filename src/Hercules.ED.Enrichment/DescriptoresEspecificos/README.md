# Description

This project contains the scripts needed to train the models for extraction of specific descriptors. It also contains the instructions to download all the required data. We train models for scientific papers, protocol papers and code projects.


# Setting up the environment

Create and load a new Python3 virtual environment:

```
virtualenv -p python3 venv
source venv/bin/activate
```

Install the dependencies and download the Spacy model:

```
pip install -r requirements.txt
spacy download en_core_web_lg
```

# Training the models

In this section we'll describe how to train the different models for papers, protocols and code projects. The training is made in two steps: first we identify and extract the keyword candidates from the dataset following several grammatical and syntactical rules, and then we train a classifier to choose the actual keywords from those candidates.


## Scientific papers

We will use the [Krapivin dataset](https://github.com/boudinfl/krapivin-2009-pre) dataset to train the model for extracting specific descriptors from scientific papers. Krapivin is a dataset of papers and their keywords.

Download and extract the dataset:

```
wget https://storage.googleapis.com/elhuyar/Hercules/specific_descriptors/krapivin-raw.tar.gz
tar xfz krapivin-raw.tar.gz
```

The tf-idf models are also required in order to train the models:

```
wget https://storage.googleapis.com/elhuyar/Hercules/specific_descriptors/hercules-tfidf-models.tar.gz
tar xfz hercules-tfidf-models.tar.gz
```

Optionally, you can also download the resources which will be created after following the steps in this guide.

```
wget https://storage.googleapis.com/elhuyar/Hercules/specific_descriptors/krapivin-datasets.tar.gz
tar xfz krapivin-datasets.tar.gz
```

Finally, the already pretrained models can be found in the main resource file referenced in the section `ServicioAPI`:

```
wget https://storage.googleapis.com/elhuyar/Hercules/hercules-models.tar.gz
```

### Extracting features, training and evaluating the models

The first step consists in extract keyword candidates and their features from the Krapivin dataset:

```
python3 krapivin_extract_features.py [--fulltext] krapivin-set/ tfidf_models/clef_en.pkl tfidf_models/scopus.pkl tfidf_models/idfakCLEF_en.pkl krapivin.tsv
```

_Note: You should set the optional argument `--fulltext` if you want to extract keyword candidates from the full texts, and leave it blank if you want to extract them only from the title + abstract._

Then split the full dataset into three sets: train, test and dev. You must not mix candidates from each document in different sets. That means you must split the dataset by document IDs. You can use the following script:

```
python3 split_train_test.py krapivin.tsv train_docs.txt test_docs.txt
```

_Note: We shared the distribution of the document IDs used in our experiments. You can find them in the files [`train_docs.txt`](https://storage.googleapis.com/elhuyar/Hercules/specific_descriptors/train_docs.txt) and [`test_docs.txt`](https://storage.googleapis.com/elhuyar/Hercules/specific_descriptors/test_docs.txt)._

Once we have the three sets, we are ready to train the model:

```
python3 train_eval_krapivin.py [--fulltext] krapivin.train.tsv krapivin.test.tsv
```

_Note: Again, set the `--fulltext` argument if you are training a model using the candidates extracted from the full texts._
_Note: Two models will be created: `krapivin_single_model.sav` and `krapivin_multi_model.sav`. The first one will be used to classify single-word candidates, and the second one to classify the multiword candidates._
_Note: Only one model is trained, with english data, and it is used both for english and spanish texts.

The script used to train the model also performs the evaluation using the test-set. Once you get the model, if you want to evaluate it separately, execute the following script:

```
python3 eval_krapivin.py --single krapivin_single_model.sav --multi krapivin_multi_model.sav krapivin.test.tsv
```

In order to get the ranking of a single document contained in the test-set, execute the following script:

```
python3 dump_doc_ranks.py krapivin_single_model.sav krapivin_multi_model.sav krapivin.test.tsv 263883.txt
```

Both models (fulltext and abstracts) will be used by the main API service to perform the keyword extraction from new text documents.


## Protocols

Work in progress.


## Code projects

Work in progress.
