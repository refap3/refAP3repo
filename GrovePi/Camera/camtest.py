#!/usr/bin/python 

# camera to twitter 
import datetime 
import time
import subprocess

ts=time.time()
now=datetime.datetime.fromtimestamp(ts).strftime('%Y%m%d-%H%M%S')
photo='~/tmp/' + now + '.jpg'
cmd = 'raspistill -t 500 -w 1024 -h 768 -o '+photo # do NOT forget to create this directory !
subprocess.call ([cmd], shell=True)

