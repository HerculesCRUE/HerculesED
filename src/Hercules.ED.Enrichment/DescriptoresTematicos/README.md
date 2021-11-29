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
wget https://storage.googleapis.com/elhuyar/Hercules/thematic_descriptors/papers-datasets.tar.gz
tar xfz papers-datasets.tar.gz
```

Finally, the already pretrained models can be found in the main resource file referenced in the section `ServicioAPI`:

```
wget https://storage.googleapis.com/elhuyar/Hercules/hercules-models.tar.gz
```

# Training the models

We train three different models, one for each level of the taxonomy: L0, L1 and L2.

We provide the train, test and dev sets of each dataset corresponding to each one of the taxonomy levels.

First you must set up a directory (referred as DATA_PATH through this file) for each level containing the following resources:

```
papers_lX.v1.train.oversampled.compact.tsv
papers_lX.v1.test.compact.tsv
papers_lX.v1.dev.compact.tsv
labels_lX.txt
```

Then you must create a symbolic link for each resource with the following exact names:
```
train.csv_ml.csv -> papers_lX.v1.train.oversampled.compact.tsv
test.csv_ml.csv -> papers_lX.v1.test.compact.tsv
dev.csv_ml.csv -> papers_lX.v1.dev.compact.tsv
labels.txt -> labels_lX.txt
```

This is how you can execute the script to train the models:

```
bash transformers_finetune.sh 0 bert_base bert-base-multilingual-cased DATA_PATH 16 2
```

This is the meaning of each argument:

```
1. Visible gpus number (0|1|...). if no gpu pass "" value
2. Transformer type = xlm-roberta| bert| ... check hugginface documentation.
3. Transformer model base directory, or hugginface model valid name xlm-roberta-base | bert-base-cased | bert-base-uncased | ...
4. Train/dev/test corpus dir.
5. Train batch size (bs) [1,2,4,8,16,32,...]
6. Gradient accumulation (ga) (int).
   IMPORTANT: gradient accumulation x train batch size = real train batch size, e.g.:
   4(bs) x 4 (ga) = 16 batch size or 16(bs) x 2 (ga) = 32 batch size.
   Lower bs = less memory usage, but slower training.
```

The model will be created inside DATA_PATH, in a directory named as `output-bert_base-eX-Y`.
