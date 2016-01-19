#!/usr/bin/python 

# detect bugger face and tweet  
import datetime 
import time
import subprocess
import tweepy 

from grovepi import *



# Connect the Grove Ultrasonic Ranger to digital port D7
ultrasonic_ranger = 7
trigger = 1000  # trigger distance in cm  i.e. ALWAYS rely on face detection 

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
api.update_status("Bugger face detection start at: " + now)

print "Twitter Connected"

from SimpleCV import Camera, Display

myCamera=Camera(0)

while True:
	try:
		# Read distance value from Ultrasonic
		distant = ultrasonicRead(ultrasonic_ranger)
		if distant <= trigger:
			print 'Alarm ', distant,'cm'

			frame=myCamera.getImage()
			faces=frame.findHaarFeatures('face')
			if faces:
				fct=0
				ts = time.time()
				now = datetime.datetime.fromtimestamp(ts).strftime('%Y%m%d-%H%M%S')

				for face in faces:
					fct+=1
					print "face " + str(fct) + " at: " + str(face.coordinates())
					myFace=face.crop()  # tweet all faces ...
					photo='/home/pi/tmp/' + now + 'F-' + str(fct) + '.jpg' # NOTE MUST use absolute path here!
					psize=myFace.width*myFace.height 
					if psize>20000: #looks like smaller images are thrash 
						print "Photo Size: " + photo + " " + str(psize)
						myFace.save(photo)
						time.sleep(1) # wait save complete ...
						status = 'OMG I have seen a buggers face! ' + now
						# tweet ...
						api.update_with_media(photo, status=status)
					else:
						print "Faced skipped too small: " + str(psize)

				print 'Sleep before next watch cycle ...'
				time.sleep(10) # sleep before next check 
		else:
			print 'No Alarm ', distant,'cm'
			time.sleep(1)
	except TypeError:
		print "Error"
	except IOError:
		print "Error"



