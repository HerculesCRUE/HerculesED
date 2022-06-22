#!/bin/bash

if [ -z "$1" ]
  then
    echo "No argument supplied, port number is required"
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

cd ${DIR}
export FLASK_APP=app.py
export FLASK_ENV=production
source venv/bin/activate
flask run --host=0.0.0.0 --port=$1 --eager-loading
