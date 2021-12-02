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

We will use the Krapivin dataset to train the model. Download and extract the dataset:

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

# Extracting features, training and evaluating the models

To create the dataset to train the model, we will use the raw Krapivin dataset and extract keyword candidates and their features from the papers included in the dataset:

```
python3 krapivin_extract_features.py [--fulltext] krapivin-set/ clef.pkl scopus.pkl idfakCLEF.pkl krapivin.tsv
```

_Note: You should set the optional argument `--fulltext` if you want to extract keyword candidates from the full texts, and leave it blank if you want to extract them only from the title + abstract._

Then split the full dataset into three sets: train, test and dev. You must not mix candidates from each document in different sets. That means you must split the dataset by document IDs. You can use the following script for that task:

```
python3 split_train_test.py krapivin.tsv train_docs.txt test_docs.txt
```

_Note: We shared the distribution of the document IDs used in our experiments. You can find them in the files [`train_docs.txt`](https://storage.googleapis.com/elhuyar/Hercules/specific_descriptors/train_docs.txt) and [`test_docs.txt`](https://storage.googleapis.com/elhuyar/Hercules/specific_descriptors/test_docs.txt)._

Once we have the three sets, we are able to train the model:

```
python3 train_eval_krapivin.py [--fulltext] krapivin.train.tsv krapivin.test.tsv
```

_Note: Again, set the `--fulltext` argument if you are training a model using the candidates extracted from the full texts._
_Note: Two models will be created: `krapivin_single_model.sav` and `krapivin_multi_model.sav`. The first one will be used to classify single-word candidates, and the second one to classify the multiword candidates._

The script used to train the model also performs the evaluation using the test-set. Once you get the model, if you want to evaluate it separately, execute the following script:

```
python3 eval_krapivin.py --single krapivin_single_model.sav --multi krapivin_multi_model.sav krapivin.test.tsv
```

In order to get the ranking of a single document contained in the test-set, execute the following script:

```
python3 dump_doc_ranks.py krapivin_single_model.sav krapivin_multi_model.sav krapivin.test.tsv 608626.txt
```

Both models (fulltext and abstracts) will be used by the main API service to perform the keyword extraction from new text documents.
