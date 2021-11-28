#!/bin/bash

### parameters:

# 1. Visible gpus number (0|1|...). if no gpu pass "" value
# 2. Transformer type = xlm-roberta| bert| ... check hugginface documentation.
# 3. Transformer model base directory, or hugginface model valid name xlm-roberta-base | bert-base-cased | bert-base-uncased | ...
# 4. Train/dev/test corpus dir.
# 5. Train batch size (bs) [1,2,4,8,16,32,...]
# 6. Gradient accumulation (ga) (int).
#    IMPORTANT: gradient accumulation x train batch size = real train batch size, e.g.:
#    4(bs) x 4 (ga) = 16 batch size or 16(bs) x 2 (ga) = 32 batch size.
#    Lower bs = less memory usage, but slower training.

export CUDA_VISIBLE_DEVICES=$1
export MAX_LENGTH=512
export TRAIN_BATCH_SIZE=$5 #1  #32
export GRADIENT_ACUMMULATION=$6 #8
export LEARNING_RATE=3e-5    #5e-5
export EPOCHS=4
export TRANSFORMER_TYPE_LONG=$2 #xlm-roberta # bert
export TRANSFORMER_BASE_DIR=$3  #xlm-roberta-base | bert-base-cased | bert-base-uncased |
export CORPUS_DIR=$4
export RESULTS_DIR=$CORPUS_DIR"/output-"$TRANSFORMER_TYPE_LONG"-e"$EPOCHS

export TRANSFORMER_BASE_FILENAME=${TRANSFORMER_BASE_DIR##*/}
export TRANSFORMER_TYPE=${TRANSFORMER_TYPE_LONG%_*}

export SAVE_STEPS=5000

echo -e "Parameters: 
     \n gpu_id=$CUDA_VISIBLE_DEVICES 
     \n max sequence length: $MAX_LENGTH
     \n train batch size: $TRAIN_BATCH_SIZE 
     \n Gradient accumulation step: $GRADIENT_ACUMMULATION 
     \n learning rate: $LEARNING_RATE
     \n epochs : $EPOCHS
     \n transformer type: $TRANSFORMER_TYPE
     \n transformer dir or code: $TRANSFORMER_BASE_DIR
     \n corpus dir: $CORPUS_DIR
     \n output dir: $RESULTS_DIR
     \n save steps: $SAVE_STEPS
     \n "

# delete cached train/test/devs for the current transformer settings in they exist  
#rm -f $CORPUS_DIR"/cached_"*"_"$TRANSFORMER_BASE_FILENAME"_"$MAX_LENGTH
#create output directory for current run
mkdir -p $RESULTS_DIR-$i


RANDOM_SEED=`echo $(($RANDOM))`

echo "********************* RUN $i - random seed: $RANDOM_SEED ************************************"

# run finetune
python -u run_classification_multilabel.py --data_dir=$CORPUS_DIR  \
       --model_type $TRANSFORMER_TYPE \
       --model_name_or_path $TRANSFORMER_BASE_DIR \
       --output_dir=$RESULTS_DIR-$i \
       --max_seq_length $MAX_LENGTH \
       --num_train_epochs=$EPOCHS \
       --per_gpu_train_batch_size $TRAIN_BATCH_SIZE \
       --gradient_accumulation_steps $GRADIENT_ACUMMULATION \
       --per_gpu_eval_batch_size 2 \
       --learning_rate $LEARNING_RATE \
       --save_steps $SAVE_STEPS \
       --logging_steps $SAVE_STEPS \
       --evaluate_during_training \
       --save_total_limit 3 \
       --labels=$CORPUS_DIR"/labels.txt" \
       --seed=$RANDOM_SEED \
       --overwrite_cache \
       --do_train \
       --do_eval \
       --do_predict



