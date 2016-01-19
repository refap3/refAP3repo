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
# NEW account - NOT itirockz any more !
consumer_key = 'x6ZHUvQFg2MudvbA5RCNHyPrs'  
consumer_secret = 'VNzZqd9cXT03gSOaku6l2uNfj7JSVaYeRIv7yr4T2vCE1mTDIu'  
access_token = '23412757-SSbIoMaqCuSyZRUQxp7jq1inhP7CQxYjHTQR3x1MG'  
access_token_secret = 'Eqzrcbfose5mEn0i3TDQMwWWVnRfZZhVhaJNjMFLi0ZF1'  

# OAuth process, using the keys and tokens  
auth = tweepy.OAuthHandler(consumer_key, consumer_secret)  
auth.set_access_token(access_token, access_token_secret)  
   
# Creation of the actual interface, using authentication  
api = tweepy.API(auth) 
ts = time.time()
now = datetime.datetime.fromtimestamp(ts).strftime('%Y%m%d-%H%M%S')
api.update_status("Bugger face detection start at: " + now)

print "Twitter Connected"

from SimpleCV import Camera, Display, DrawingLayer, Color

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
						myDL=DrawingLayer((myFace.width,myFace.height))
						myDL.setFontSize(25)
						myDL.text("BUGGER",(myFace.width/2 -20,10),color=Color.WHITE)
						myFace.addDrawingLayer(myDL)
						myFace.applyLayers()
						
						myFace.save(photo)
						time.sleep(1) # wait save complete ...
						status = 'OMG I have seen a buggers face! ' + now
						# tweet ...
						api.update_with_media(photo, status=status)
					else:
						print "Faced skipped too small: " + str(psize)

				print 'Sleep before next watch cycle ...'
				time.sleep(2) # sleep before next check 
		else:
			print 'No Alarm ', distant,'cm'
			time.sleep(1)
	except TypeError:
		print "Error"
	except IOError:
		print "Error"



