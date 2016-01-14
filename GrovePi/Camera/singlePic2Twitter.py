#!/usr/bin/python 

# camera to twitter 
import datetime 
import time
import subprocess
import tweepy 

# Consumer keys and access tokens, used for OAuth  
consumer_key = 'qpsCnt3kZxKMu6H0giJaBxyzd'  
consumer_secret = 'yTX9WxGqX6mZYxVgSGPBCAchH4lvxaism8yBHP08uGvnOZNVBa'  
access_token = '1249046982-KZ8e86QyUfe9dRWmtVRTTvhUEUDReFrS1ODMfq9'  
access_token_secret = 'n9iyZ009L0RdRN030HtHgQiB0tsEClvOnAldzTqaV96JC'  

# OAuth process, using the keys and tokens  
auth = tweepy.OAuthHandler(consumer_key, consumer_secret)  
auth.set_access_token(access_token, access_token_secret)  
   
# Creation of the actual interface, using authentication  
api = tweepy.API(auth) 
ts = time.time()
now = datetime.datetime.fromtimestamp(ts).strftime('%Y%m%d-%H%M%S')
#api.update_status("Session start at: " + now)

print "Twitter Connected"

photo='/home/pi/tmp/' + now + '.jpg' # NOTE MUST use absolute path here!
cmd = 'raspistill -t 500 -w 1024 -h 768 -o ' + photo # do NOT forget to create this directory !
subprocess.call ([cmd], shell=True)

print 'Sleep be4 tweet'
time.sleep(2)
status = 'Single Photo-tweet ' + now

# tweet ...
api.update_with_media(photo, status=status)

