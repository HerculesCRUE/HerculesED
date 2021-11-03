# Description

This module contains two different systems to train and evaluate multilabel classifiers to predict thematic descriptors from research objects. One of the systems is based on traditional machine learning techniques like Logistic Regression and SVM. The other system is a neural classifier based on Multilabel Bert language model.

**Note**: The datasets are currently in a private storage bucket and are not available yet.


# Logistic Regression and SVM based classifiers

```
python3 multilabel_classification_lr_svm.py TRAIN_TSV TEST_TSV ALGORITHM
```


# Multilingual-Bert based neural classifiers

The script file is `multilabel_classification_bert.ipynb`.

Notes:
- Currently the script is a ipynb file, meaning it should be executed as a Python notebook. The development has been made in Google Colab.
- The script can be executed in a Google Colab TPU.
- Currently the datasets are loaded from a private Google Bucket.



