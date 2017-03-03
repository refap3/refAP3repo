#!/usr/bin/python 

# call like this: 
# /home/pi/refAP3repo/GrovePi/Camera/camstit17.py


url="http://stit17.azurewebsites.net:80/api/alexa/kweki"

try:
    # For Python 3.0 and later
    from urllib.request import urlopen
except ImportError:
    # Fall back to Python 2's urllib2
    from urllib2 import urlopen

import json

# imported from faces2twitter for STIT 17 
import datetime 
import time
import subprocess
import tweepy 

from grovepi import *
from grove_rgb_lcd import *
from SimpleCV import Camera, Display, DrawingLayer, Color

# GrovePi connections ...
ultrasonic_ranger = 7 #D7
trigger = 50  # trigger distance in cm  
buzzer_pin=2 # D2 connect buzzer here - will confirm face detected
led=5 # D5 
button=6 # D6 
# LCD on any I2C Port 


# T W I T T E R: 
# Consumer keys and access tokens, used for OAuth  
twa=0 # set this to 0 to use lecko account OTHERWISE itirockz is used !
#--------------------------------------------------------------------------
if (twa==0):
    consumer_key = 'x6ZHUvQFg2MudvbA5RCNHyPrs'  
    consumer_secret = 'VNzZqd9cXT03gSOaku6l2uNfj7JSVaYeRIv7yr4T2vCE1mTDIu'  
    access_token = '23412757-SSbIoMaqCuSyZRUQxp7jq1inhP7CQxYjHTQR3x1MG'  
    access_token_secret = 'Eqzrcbfose5mEn0i3TDQMwWWVnRfZZhVhaJNjMFLi0ZF1'  
else:
    consumer_key = 'qpsCnt3kZxKMu6H0giJaBxyzd'  
    consumer_secret = 'yTX9WxGqX6mZYxVgSGPBCAchH4lvxaism8yBHP08uGvnOZNVBa'  
    access_token = '1249046982-KZ8e86QyUfe9dRWmtVRTTvhUEUDReFrS1ODMfq9'  
    access_token_secret = 'n9iyZ009L0RdRN030HtHgQiB0tsEClvOnAldzTqaV96JC'  
    
# OAuth process, using the keys and tokens  
auth = tweepy.OAuthHandler(consumer_key, consumer_secret)  
auth.set_access_token(access_token, access_token_secret)  
   
# Creation of the actual interface, using authentication  
api = tweepy.API(auth) 

# camera:
myCamera=Camera(0,  {"width":1024, "height":768})


def beep(duration):
    digitalWrite(buzzer_pin,1)
    time.sleep(duration)
    digitalWrite(buzzer_pin,0)
    
def logLCD(status):
    flushLCD(status)
    time.sleep(1)
    
def flushLCD(status):
    setText(status)

def setup():

    pinMode(buzzer_pin,"OUTPUT")
    pinMode(led,"OUTPUT")
    pinMode(button,"OUTPUT")

    print "Twitter Connected"
    beep(0.01)

def get_jsonparsed_data(url):
    """
    Receive the content of ``url``, parse it as JSON and return the object.

    Parameters
    ----------
    url : str

    Returns
    -------
    dict
	
	Call like this: 
	-----------------
	j=get_jsonparsed_data(url)
	print j
	print j["Ort"]
	
    """
    response = urlopen(url)
    data = response.read().decode("utf-8")
    return json.loads(data)

	


def loop():
	try:
		json=get_jsonparsed_data(url)
		setRGB(0,128,64)
		setRGB(0,255,0)
		# Read distance value from Ultrasonic
		distant = ultrasonicRead(ultrasonic_ranger)
		button_state=digitalRead(button)
		flushLCD('+++ ' + str(distant) + ':'  + str(trigger))

		analogWrite(led,255)
		# count down for photo !
		flushLCD('SMILE!')
		beep (0.01)
		time.sleep (1)
		beep (0.02)
		time.sleep(1)
		beep (0.1)
		time.sleep (1)
		
		frame=myCamera.getImage()
		flushLCD('processing ...')
		ts = time.time()
		now = datetime.datetime.fromtimestamp(ts).strftime('%Y%m%d-%H%M%S')
		photo='/home/pi/tmp/' + now + '.jpg' # NOTE MUST use absolute path here!
		psize=frame.width*frame.height 
		print "Photo Size: " + photo + " " + str(psize) + " w:" + str(frame.width) + " h: " + str(frame.height)
		myDL=DrawingLayer((frame.width,frame.height))
		myDL.setFontSize(45)
		myDL.text("Quake report from " + json["Ort"] + " with Strength of " + json["LastStrengthString"] + " at: " + json["LastOccured"] ,(10,10),color=Color.RED)
		frame.addDrawingLayer(myDL)
		frame.applyLayers()
		
		frame.save(photo)
		beep (0.05)
		time.sleep(0.3)
		beep(0.2)

		time.sleep(1) # wait save complete ...
		status = 'Look Ma, I did the #stit17 @ #FHburgenland just now: ' + now
		status = 'I did stitedt ITTed ' + now
		# tweet ...
		api.update_with_media(photo, status=status)
		logLCD('TWEETed!')

	
	except TypeError:
		print "Type Error"
	except IOError:
		print "IO Error"

def destroy():
    print 'DONE looking for faces ...'
    digitalWrite(buzzer_pin,0)
    pass
	
def work(): 
    setup()
    try:
        loop()
    except KeyboardInterrupt: 
        destroy()
	
        
if __name__ == '__main__':
	work()
               
        



