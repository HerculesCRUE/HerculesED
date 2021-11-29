#!/usr/bin/python3 -u
import os

instance_path='/mnt/EBS/gnoss_bbva_api'
#activate_this = os.path.join(instance_path,'venv','bin','activate_this.py')
#with open(activate_this) as f:
#    exec(f.read(), dict(__file__=activate_this))

import sys
sys.path.insert(0, instance_path)
from app import app as application
