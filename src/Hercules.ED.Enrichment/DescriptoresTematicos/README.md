# Description

This project uses the custom module Text-classification (referenced in requirements.txt) and previously collected and curated datasets to train classification models for scientific papers, Bio-Protocol papers and SourceForge projects.

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

Download the datasets from the following URL and extract them to disk:

```
wget https://storage.googleapis.com/elhuyar/Hercules/thematic_descriptors/thematic-descriptor-datasets.tar.gz
tar xfz thematic-descriptor-datasets.tar.gz
```

Optionally, the already pretrained models can be found in the main resource file referenced in the section `ServicioAPI`:

```
wget https://storage.googleapis.com/elhuyar/Hercules/hercules-models.tar.gz
```


# Models

We train different models independently for papers, protocols and code projects. We use Arxiv, Scopus and Pubmed datasets for the papers model, Bio-Protocol dataset for protocols, and Sourceforge dataset for code projects.

In the case of papers, two versions must be trained: using full texts and using only abstracts. Furthermore, for each version of the papers dataset, we train three models, one for each different level of the topic taxonomy (L0, L1 and L2). Protocols might also be trained twice, with the full texts or only with the abstracts.

As a result, we train 9 models overall. This is the outline of the resulting models:

```
- papers (Arxiv, Scopus and Pubmed)
  - fulltexts
    - l0
    - l1
    - l2
  - abstracts
    - l0
    - l1
    - l2
- protocols (Bio-Protocol)
  - abstracts
  - fulltexts
- code projects (SourceForge)
  - title+description
```


# Training the models

We use an additional module called Text-classification to train the models. The module is installed along with the requirements defined in requirements.txt. All the datasets are already prepared to be trained by that module.

Once Text-classification module is installed, three new commands will be available in the environment. The one we'll use to train the models is called `train`.

If you followed the steps outlined at the beginning of thid README, all the datasets should already be available in a directory called `datasets/`.


## Training the model for code projects (SourceForge)

The outline of the datasets corresponding to code projects is the following:

```
- datasets/
  - sourceforge/
    - label/
      - train.tsv
      - test.tsv
      - dev.tsv
```

To train the model, we execute the following command:

```
$ train datasets/sourceforge/label/ sourceforge/ TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
```

Where the last argument `0` is the GPU ID to use for training.

The new model will be created into `sourceforge/`. The multiple `checkpoint-*` directories inside can be safely removed.


## Training the models for protocols (Bio-Protocol)

The outline of the datasets corresponding to protocols is the following:

```
- datasets/
  - bio-protocol/
    - abs/
      - categories/
      	- train.tsv
      	- test.tsv
      	- dev.tsv
    - full/
      - categories/
      	- train.tsv
	- test.tsv
	- dev.tsv
```

To train the models, we execute the following commands:
```
$ train datasets/bio-protocol/abs/categories/ bio-protocol/abs TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
$ train datasets/bio-protocol/full/categories/ bio-protocol/full TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
```

Where the last argument `0` is the GPU ID to use for training.

The new models will be created into `bio-protocol/abs` and `bio-protocol/full`. The multiple checkpoint-* directories inside can be safely removed.


## Training the models for scientific papers (Arxiv, Scopus, Pubmed)

In order to create the papers dataset, three datasets were merged: Arxiv, Scopus and Pubmed. If you followed the first steps at the beginning of this file, you will have available the final dataset, which already contains all mentioned sub-datasets inside.

```
- datasets/
  - papers/
    - abs/
      - topics_l0/
      	- train.tsv
      	- test.tsv
      	- dev.tsv
      - topics_l1/
      	- train.tsv
      	- test.tsv
      	- dev.tsv
      - topics_l2/
      	- train.tsv
      	- test.tsv
      	- dev.tsv
    - full/
      - topics_l0/
      	- train.tsv
      	- test.tsv
      	- dev.tsv
      - topics_l1/
      	- train.tsv
      	- test.tsv
      	- dev.tsv
      - topics_l2/
      	- train.tsv
      	- test.tsv
      	- dev.tsv
```

To train the models, we execute the following commands:
```
$ train datasets/papers/abs/topics_l0 papers/abs/l0 TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
$ train datasets/papers/abs/topics_l1 papers/abs/l1 TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
$ train datasets/papers/abs/topics_l2 papers/abs/l2 TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
$ train datasets/papers/full/topics_l0 papers/full/l0 TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
$ train datasets/papers/full/topics_l1 papers/full/l1 TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
$ train datasets/papers/full/topics_l2 papers/full/l2 TRANSFORMERS --epochs 10 --balance class bert-base-cased multi-label 0
```

Where the last argument `0` is the GPU ID to use for training.

The new models will be created into `datasets/papers/{abs,full}/{l0,l1,l2}`. The multiple `checkpoint-*` directories inside can be safely removed.
