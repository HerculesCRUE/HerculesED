# coding=utf-8
# Copyright 2018 The Google AI Language Team Authors and The HuggingFace Inc. team.
# Copyright (c) 2018, NVIDIA CORPORATION.  All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from __future__ import absolute_import, division, print_function

import csv
import json
import os
import sys
from collections import Counter
from io import open
from multiprocessing import Pool, cpu_count

import torchvision
import torchvision.transforms as transforms
from PIL import Image
from scipy.stats import pearsonr, spearmanr
from sklearn.metrics import f1_score, matthews_corrcoef
from tqdm.auto import tqdm

import torch
import torch.nn as nn
from torch.utils.data import Dataset

import random
import numpy as np
import re
import glob


import logging

logger = logging.getLogger(__name__)

csv.field_size_limit(2147483647)

device=None


from transformers import (
    MODEL_FOR_SEQUENCE_CLASSIFICATION_MAPPING,
    WEIGHTS_NAME,
    AdamW,
    AutoConfig,
    AutoModelForSequenceClassification,
    AutoTokenizer,
    get_linear_schedule_with_warmup,
)

try:
    from torch.utils.tensorboard import SummaryWriter
except ImportError:
    from tensorboardX import SummaryWriter


def set_seed(s,n_gpu):
    random.seed(s)
    np.random.seed(s)
    torch.manual_seed(s)
    if n_gpu > 0:
        torch.cuda.manual_seed_all(s)


def get_labels(path):
    if path:
        with open(path, "r") as f:
            labels = f.read().splitlines()
        return labels
    else:
        #return ["P", "N","NEU","NONE"]
        return ["pos","neg","neu"]






class InputExample(object):
    """A single training/test example for simple sequence classification."""

    def __init__(self, guid, text_a, text_b=None, label=None):
        """
        Constructs a InputExample.

        Args:
            guid: Unique id for the example.
            text_a: string. The untokenized text of the first sequence. For single
            sequence tasks, only this sequence must be specified.
            text_b: (Optional) string. The untokenized text of the second sequence.
            Only must be specified for sequence pair tasks.
            label: (Optional) string. The label of the example. This should be
            specified for train and dev examples, but not for test examples.
        """

        self.guid = guid
        self.text_a = text_a
        self.text_b = text_b
        self.label = label


class InputFeatures(object):
    """A single set of features of data."""

    def __init__(self, input_ids, input_mask, segment_ids, label_ids):
        self.input_ids = input_ids
        self.input_mask = input_mask
        self.segment_ids = segment_ids
        self.label_ids = label_ids


def convert_example_to_feature(
    example_row,
    label_map,
    pad_token=0,
    sequence_a_segment_id=0,
    sequence_b_segment_id=1,
    cls_token_segment_id=1,
    pad_token_segment_id=0,
    mask_padding_with_zero=True,
    sep_token_extra=False,
):
    (
        example,
        max_seq_length,
        tokenizer,
        output_mode,
        cls_token_at_end,
        cls_token,
        sep_token,
        cls_token_segment_id,
        pad_on_left,
        pad_token_segment_id,
        sep_token_extra,
        multi_label,
        stride,
    ) = example_row

    

    sys.stderr.write("current example to tokenize: {}\n".format(example.text_a))
    tokens_a = tokenizer.tokenize(example.text_a)

    tokens_b = None
    if example.text_b:
        tokens_b = tokenizer.tokenize(example.text_b)
        # Modifies `tokens_a` and `tokens_b` in place so that the total
        # length is less than the specified length.
        # Account for [CLS], [SEP], [SEP] with "- 3". " -4" for RoBERTa.
        special_tokens_count = 4 if sep_token_extra else 3
        _truncate_seq_pair(tokens_a, tokens_b, max_seq_length - special_tokens_count)
    else:
        # Account for [CLS] and [SEP] with "- 2" and with "- 3" for RoBERTa.
        special_tokens_count = 3 if sep_token_extra else 2
        if len(tokens_a) > max_seq_length - special_tokens_count:
            tokens_a = tokens_a[: (max_seq_length - special_tokens_count)]

    # The convention in BERT is:
    # (a) For sequence pairs:
    #  tokens:   [CLS] is this jack ##son ##ville ? [SEP] no it is not . [SEP]
    #  type_ids:   0   0  0    0    0     0       0   0   1  1  1  1   1   1
    # (b) For single sequences:
    #  tokens:   [CLS] the dog is hairy . [SEP]
    #  type_ids:   0   0   0   0  0     0   0
    #
    # Where "type_ids" are used to indicate whether this is the first
    # sequence or the second sequence. The embedding vectors for `type=0` and
    # `type=1` were learned during pre-training and are added to the wordpiece
    # embedding vector (and position vector). This is not *strictly* necessary
    # since the [SEP] token unambiguously separates the sequences, but it makes
    # it easier for the model to learn the concept of sequences.
    #
    # For classification tasks, the first vector (corresponding to [CLS]) is
    # used as as the "sentence vector". Note that this only makes sense because
    # the entire model is fine-tuned.
    tokens = tokens_a + [sep_token]
    segment_ids = [sequence_a_segment_id] * len(tokens)

    if tokens_b:
        if sep_token_extra:
            tokens += [sep_token]
            segment_ids += [sequence_b_segment_id]

        tokens += tokens_b + [sep_token]

        segment_ids += [sequence_b_segment_id] * (len(tokens_b) + 1)

    if cls_token_at_end:
        tokens = tokens + [cls_token]
        segment_ids = segment_ids + [cls_token_segment_id]
    else:
        tokens = [cls_token] + tokens
        segment_ids = [cls_token_segment_id] + segment_ids

    input_ids = tokenizer.convert_tokens_to_ids(tokens)

    # The mask has 1 for real tokens and 0 for padding tokens. Only real
    # tokens are attended to.
    input_mask = [1 if mask_padding_with_zero else 0] * len(input_ids)

    # Zero-pad up to the sequence length.
    padding_length = max_seq_length - len(input_ids)
    if pad_on_left:
        input_ids = ([pad_token] * padding_length) + input_ids
        input_mask = ([0 if mask_padding_with_zero else 1] * padding_length) + input_mask
        segment_ids = ([pad_token_segment_id] * padding_length) + segment_ids
    else:
        input_ids = input_ids + ([pad_token] * padding_length)
        input_mask = input_mask + ([0 if mask_padding_with_zero else 1] * padding_length)
        segment_ids = segment_ids + ([pad_token_segment_id] * padding_length)

    assert len(input_ids) == max_seq_length
    assert len(input_mask) == max_seq_length
    assert len(segment_ids) == max_seq_length

    # if output_mode == "classification":
    #     label_id = label_map[example.label]
    # elif output_mode == "regression":
    #     label_id = float(example.label)
    # else:
    #     raise KeyError(output_mode)

    # if output_mode == "regression":
    #     label_id = float(example.label)

    sys.stderr.write("label {} for example {}\n".format(example.label,example.guid))
    #label_ids = sorted([label_map[l] for l in example.label.split('|')])
    label_ids=[]
    label_multihot = [1 if i in label_ids else 0 for i in range(0,len(label_map))]
    #sys.stderr.write("label_ids: {}".format(label_ids))
    #sys.stderr.write("label_id {} for example {}\n".format(label_id,example.text_a))
    
    return InputFeatures(input_ids=input_ids, input_mask=input_mask, segment_ids=segment_ids, label_ids=label_multihot,)


def convert_example_to_feature_sliding_window(
    example_row,
    pad_token=0,
    sequence_a_segment_id=0,
    sequence_b_segment_id=1,
    cls_token_segment_id=1,
    pad_token_segment_id=0,
    mask_padding_with_zero=True,
    sep_token_extra=False,
):
    (
        example,
        max_seq_length,
        tokenizer,
        output_mode,
        cls_token_at_end,
        cls_token,
        sep_token,
        cls_token_segment_id,
        pad_on_left,
        pad_token_segment_id,
        sep_token_extra,
        multi_label,
        stride,
    ) = example_row

    if stride < 1:
        stride = int(max_seq_length * stride)

    bucket_size = max_seq_length - (3 if sep_token_extra else 2)
    token_sets = []

    tokens_a = tokenizer.tokenize(example.text_a)

    if len(tokens_a) > bucket_size:
        token_sets = [tokens_a[i : i + bucket_size] for i in range(0, len(tokens_a), stride)]
    else:
        token_sets.append(tokens_a)

    if example.text_b:
        raise ValueError("Sequence pair tasks not implemented for sliding window tokenization.")

    # The convention in BERT is:
    # (a) For sequence pairs:
    #  tokens:   [CLS] is this jack ##son ##ville ? [SEP] no it is not . [SEP]
    #  type_ids:   0   0  0    0    0     0       0   0   1  1  1  1   1   1
    # (b) For single sequences:
    #  tokens:   [CLS] the dog is hairy . [SEP]
    #  type_ids:   0   0   0   0  0     0   0
    #
    # Where "type_ids" are used to indicate whether this is the first
    # sequence or the second sequence. The embedding vectors for `type=0` and
    # `type=1` were learned during pre-training and are added to the wordpiece
    # embedding vector (and position vector). This is not *strictly* necessary
    # since the [SEP] token unambiguously separates the sequences, but it makes
    # it easier for the model to learn the concept of sequences.
    #
    # For classification tasks, the first vector (corresponding to [CLS]) is
    # used as as the "sentence vector". Note that this only makes sense because
    # the entire model is fine-tuned.

    input_features = []
    for tokens_a in token_sets:
        tokens = tokens_a + [sep_token]
        segment_ids = [sequence_a_segment_id] * len(tokens)

        if cls_token_at_end:
            tokens = tokens + [cls_token]
            segment_ids = segment_ids + [cls_token_segment_id]
        else:
            tokens = [cls_token] + tokens
            segment_ids = [cls_token_segment_id] + segment_ids

        input_ids = tokenizer.convert_tokens_to_ids(tokens)

        # The mask has 1 for real tokens and 0 for padding tokens. Only real
        # tokens are attended to.
        input_mask = [1 if mask_padding_with_zero else 0] * len(input_ids)

        # Zero-pad up to the sequence length.
        padding_length = max_seq_length - len(input_ids)
        if pad_on_left:
            input_ids = ([pad_token] * padding_length) + input_ids
            input_mask = ([0 if mask_padding_with_zero else 1] * padding_length) + input_mask
            segment_ids = ([pad_token_segment_id] * padding_length) + segment_ids
        else:
            input_ids = input_ids + ([pad_token] * padding_length)
            input_mask = input_mask + ([0 if mask_padding_with_zero else 1] * padding_length)
            segment_ids = segment_ids + ([pad_token_segment_id] * padding_length)

        assert len(input_ids) == max_seq_length
        assert len(input_mask) == max_seq_length
        assert len(segment_ids) == max_seq_length

        # if output_mode == "classification":
        #     label_id = label_map[example.label]
        # elif output_mode == "regression":
        #     label_id = float(example.label)
        # else:
        #     raise KeyError(output_mode)

        input_features.append(
            InputFeatures(input_ids=input_ids, input_mask=input_mask, segment_ids=segment_ids, label_id=example.label,)
        )

    return input_features


def convert_examples_to_features_multi(
    examples,
    label_map,
    max_seq_length,
    tokenizer,
    output_mode="classification",
    cls_token_at_end=False,
    sep_token_extra=False,
    pad_on_left=False,
    cls_token="[CLS]",
    sep_token="[SEP]",
    pad_token=0,
    sequence_a_segment_id=0,
    sequence_b_segment_id=1,
    cls_token_segment_id=1,
    pad_token_segment_id=0,
    mask_padding_with_zero=True,
    process_count=cpu_count() - 2,
    multi_label=False,
    silent=False,
    use_multiprocessing=False,
    sliding_window=False,
    flatten=False,
    stride=None,
):
    """ Loads a data file into a list of `InputBatch`s
        `cls_token_at_end` define the location of the CLS token:
            - False (Default, BERT/XLM pattern): [CLS] + A + [SEP] + B + [SEP]
            - True (XLNet/GPT pattern): A + [SEP] + B + [SEP] + [CLS]
        `cls_token_segment_id` define the segment id associated to the CLS token (0 for BERT, 2 for XLNet)
    """

    examples = [
        (
            example,
            max_seq_length,
            tokenizer,
            output_mode,
            cls_token_at_end,
            cls_token,
            sep_token,
            cls_token_segment_id,
            pad_on_left,
            pad_token_segment_id,
            sep_token_extra,
            multi_label,
            stride,
        )
        for example in examples
    ]

       
        
    if use_multiprocessing:
        if sliding_window:
            with Pool(process_count) as p:
                features = list(
                    tqdm(
                        p.imap(convert_example_to_feature_sliding_window, examples, chunksize=500,),
                        total=len(examples),
                        disable=silent,
                    )
                )
            if flatten:
                features = [feature for feature_set in features for feature in feature_set]
        else:
            with Pool(process_count) as p:
                features = list(
                    tqdm(
                        p.imap(convert_example_to_feature, examples, label_map, chunksize=500),
                        total=len(examples),
                        disable=silent,
                    )
                )
    else:
        if sliding_window:
            features = [
                convert_example_to_feature_sliding_window(example) for example in tqdm(examples, disable=silent)
            ]
            if flatten:
                features = [feature for feature_set in features for feature in feature_set]
        else:
            features = [convert_example_to_feature(example,label_map) for example in tqdm(examples, disable=silent)]

    num_ex=len(examples)
    if num_ex > 5:
        num_ex=5
        
    for i in range(0,num_ex):
        ex_a=examples[i][0]
        logger.info("*** Example ***")
        logger.info("guid: %s", ex_a.guid)
        tokens_a = tokenizer.tokenize(ex_a.text_a)     
        logger.info("tokens: %s", " ".join([str(x) for x in tokens_a]))
        logger.info("input ids: %s", " ".join([str(x) for x in features[i].input_ids]))
        logger.info("label ids: %s", ",".join([str(x) for x in features[i].label_ids])) 
            
    return features


def _truncate_seq_pair(tokens_a, tokens_b, max_length):
    """Truncates a sequence pair in place to the maximum length."""

    # This is a simple heuristic which will always truncate the longer sequence
    # one token at a time. This makes more sense than truncating an equal percent
    # of tokens from each, since if one sequence is very short then each token
    # that's truncated likely contains more information than a longer sequence.

    while True:
        total_length = len(tokens_a) + len(tokens_b)
        if total_length <= max_length:
            break
        if len(tokens_a) > len(tokens_b):
            tokens_a.pop()
        else:
            tokens_b.pop()


POOLING_BREAKDOWN = {1: (1, 1), 2: (2, 1), 3: (3, 1), 4: (2, 2), 5: (5, 1), 6: (3, 2), 7: (7, 1), 8: (4, 2), 9: (3, 3)}


from sklearn.metrics import classification_report, confusion_matrix, multilabel_confusion_matrix
from sklearn.metrics import precision_score, f1_score, recall_score

from torch.nn import CrossEntropyLoss
from torch.utils.data import DataLoader, RandomSampler, SequentialSampler, TensorDataset
from torch.utils.data.distributed import DistributedSampler
from tqdm import tqdm, trange

from utils_classification_multilabel import set_seed,get_labels,convert_examples_to_features_multi 
from transformers.data.processors.utils import SingleSentenceClassificationProcessor as Processor

################################################################

#  Neural models related  functions

################################################################

def evaluate(model, tokenizer, labels, pad_token_label_id, mode, prefix="", input_text="", model_type="roberta"):
    input_str_array=[]
    input_str_array.insert(0,input_text)

    logger.info("***** Tagging text : ***** \n {} \n ********************** \n".format(input_str_array))
    
    
    examples = Processor.create_from_examples(input_str_array)

    logger.info("example created:  {}, converting to features... \n ".format(examples))
    
    features = convert_examples_to_features_multi(
        examples,
        labels,
        512,
        tokenizer,
        cls_token_at_end=bool(model_type in ["xlnet"]),
        # xlnet has a cls token at the end
        cls_token=tokenizer.cls_token,
        cls_token_segment_id=2 if model_type in ["xlnet"] else 0,
        sep_token=tokenizer.sep_token,
        sep_token_extra=bool(model_type in ["roberta"]),
        # roberta uses an extra separator b/w pairs of sentences, cf. github.com/pytorch/fairseq/commit/1684e166e3da03f5b600dbb7855cb98ddfcd0805
        pad_on_left=bool(model_type in ["xlnet"]),
        # pad on the left for xlnet
        pad_token=tokenizer.pad_token_id,
        pad_token_segment_id=tokenizer.pad_token_type_id,
        multi_label=True,
        #pad_token_label_id=pad_token_label_id,
    )

    logger.info("example converted to features, creating tensors... \n")

    # Convert to Tensors and build dataset
    all_input_ids = torch.tensor([f.input_ids for f in features], dtype=torch.long)
    all_input_mask = torch.tensor([f.input_mask for f in features], dtype=torch.long)
    all_segment_ids = torch.tensor([f.segment_ids for f in features], dtype=torch.long)
    all_label_ids = torch.tensor([f.label_ids+[0]*(len(labels)-len(f.label_ids)) for f in features], dtype=torch.float)

    eval_dataset = TensorDataset(all_input_ids, all_input_mask, all_segment_ids, all_label_ids)
    
    eval_batch_size = 1
    # Note that DistributedSampler samples randomly
    eval_sampler = SequentialSampler(eval_dataset) 
    eval_dataloader = DataLoader(eval_dataset, sampler=eval_sampler, batch_size=1)

    # Eval!
    logger.info("***** Running evaluation %s on  %s set*****", prefix, mode)
    logger.info("  Num examples = %d", len(eval_dataset))
    logger.info("  Batch size = %d", eval_batch_size)
    eval_loss = 0.0
    nb_eval_steps = 0
    preds, out_label_ids, logit_preds = [],[],[]
        
    model.eval()
    for batch in tqdm(eval_dataloader, desc="Evaluating"):
        batch = tuple(t.to(device) for t in batch)

        with torch.no_grad():
            inputs = {"input_ids": batch[0], "attention_mask": batch[1], "labels": batch[3]}
            if model_type != "distilbert":
                inputs["token_type_ids"] = (
                    batch[2] if model_type in ["bert", "xlnet"] else None
                )  # XLM and RoBERTa don"t use segment_ids
            outputs = model(**inputs)
            #tmp_eval_loss, logits = outputs[:2]
            tmp_eval_loss, b_logit_pred = outputs[:2]

            pred_label = torch.sigmoid(b_logit_pred)
                    
            b_logit_pred = b_logit_pred.detach().cpu().numpy()
            pred_label = pred_label.to('cpu').numpy()
            b_labels = inputs["labels"].to('cpu').numpy()
            
            eval_loss += tmp_eval_loss.item()
            
        nb_eval_steps += 1

        out_label_ids.extend(b_labels)
        preds.extend(pred_label)
            
        
    eval_loss = eval_loss / nb_eval_steps
    #preds = np.argmax(preds, axis=1)
    #sys.stderr.write("predictions: {}".format(preds))
            
    label_map = {i: label for label,i in labels.items()}

    out_label_list = [] #["" for _ in range(out_label_ids.shape[0])]
    preds_list = [] #["" for _ in range(out_label_ids.shape[0])]
    probs_list = []
    # unfold batch
    #preds=[l for one_pred in preds for l in one_pred ]
    #out_label_ids=[l for one_label in out_label_ids for l in one_label ]

    
    #sys.stderr.write("out_label_ids shape: {}\n preds shape: {}".format(out_label_ids[0],preds[0]))

    for i in range(len(out_label_ids)):
        #predictions to booleans . NOTE: threshold set to 0.50 for sigmoid this could be optimized.
        predicted = [1 if j>0.50 else 0 for j in preds[i]]
        probs = [j for j in preds[i]]
        
        out_label_list.append(out_label_ids[i])
        preds_list.append(predicted)
        probs_list.append(probs)
        
    
    return preds_list, probs_list




def init_neural_resources(where):
    # Setup CUDA, GPU & distributed training
    device = torch.device("cuda" if torch.cuda.is_available() and where != "cpu" else "cpu")
    n_gpu = torch.cuda.device_count()

    # Setup logging
    logging.basicConfig(
        format="%(asctime)s - %(levelname)s - %(name)s -   %(message)s",
        datefmt="%m/%d/%Y %H:%M:%S",
        level=logging.INFO,
    )
    logger.warning(
        "Process rank: %s, device: %s, n_gpu: %s, distributed training: %s, 16-bits training: %s",
        -1,
        device,
        n_gpu,
        False, # local_rank != -1
        False, #fp16 yes or no
    )

    # Set seed
    set_seed(1,n_gpu)







def load_neural_model(name,path,tmp_path):
    
    # prepare labels
    labels = get_labels(os.path.join(path,"labels.txt"))
    num_labels = len(labels)
    label2id={label: i for i, label in enumerate(labels)}
    # Use cross entropy ignore index as padding label id so that only real label ids contribute to the loss later
    pad_token_label_id = CrossEntropyLoss().ignore_index
    id2label={str(i): label for i, label in enumerate(labels)}
    
    try:
        config = AutoConfig.from_pretrained(
            path,
            num_labels=num_labels,
            id2label=id2label,
            label2id=label2id,
            cache_dir=tmp_path,
            problem_type="multi_label_classification"
        )
        #tokenizer_args = {k: v for k, v in vars(args).items() if v is not None and k in TOKENIZER_ARGS}
        tokenizer_args = {"do_lower_case":False} #, "strip_accents":False, "keep_accents":True, "use_fast":True}
        logger.info("Tokenizer arguments: %s", tokenizer_args)
        tokenizer = AutoTokenizer.from_pretrained(
            path,
            cache_dir=tmp_path,
            **tokenizer_args,
        )

        sys.stderr.write("tokenizer object loaded: {} {}\n".format(name, tokenizer))
        model = AutoModelForSequenceClassification.from_pretrained(
            path,
            from_tf=bool(".ckpt" in path),
            config=config,
            cache_dir=tmp_path,
        )

        return(tokenizer,model,label2id,id2label,pad_token_label_id)
    except OSError as e:
        logger.error("Directory '{}' doesn't contain pretrained data".format(path))
        return (-1,None,-1,-1,-1)
