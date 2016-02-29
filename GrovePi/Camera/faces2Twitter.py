#!/usr/bin/python 

# detect STIT faces and tweet  
import datetime 
import time
import subprocess
import tweepy 

from grovepi import *
from grove_rgb_lcd import *

def beep(duration):
    digitalWrite(buzzer_pin,1)
    time.sleep(duration)
    digitalWrite(buzzer_pin,0)
    
def logLCD(status):
    flushLCD(status)
    time.sleep(1)
    
def flushLCD(status):
    setText(status)


# Connect the Grove Ultrasonic Ranger to digital port D7
ultrasonic_ranger = 7
trigger = 50  # trigger distance in cm  -- will be overridden later 
buzzer_pin=2 # connect buzzer here - will confirm face detected
pinMode(buzzer_pin,"OUTPUT")
# connect red LED to D5 -- will turn on iff object within alarm distance
led=5
pinMode(led,"OUTPUT")

# connect the LCD display to any of the IC2 ports

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
api.update_status("STIT face detection start at: " + now)

print "Twitter Connected"
beep(0.01)
from SimpleCV import Camera, Display, DrawingLayer, Color

myCamera=Camera(0,  {"width":1024, "height":768})

while True:
    try:
        setRGB(0,128,64)
        setRGB(0,255,0)
        # Read distance value from Ultrasonic
        distant = ultrasonicRead(ultrasonic_ranger)
        if distant <= trigger:
            print 'Alarm ', distant,'cm', 'trigger', trigger
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
                        myDL.text("I am " + str(distant) + " cm next to a PiCam!",(myFace.width/2 - 140,10),color=Color.WHITE)
                        myFace.addDrawingLayer(myDL)
                        myFace.applyLayers()
                        
                        myFace.save(photo)
                        beep (0.05)
                        time.sleep(0.3)
                        beep(0.2)

                        time.sleep(1) # wait save complete ...
                        status = 'LOOK MA, I did the STIT! ' + now
                        # tweet ...
                        api.update_with_media(photo, status=status)
                        logLCD('TWEETed!')

                    else:
                        print "Face skipped too small: " + str(psize)
                        logLCD("Face " + str(fct) + " skipped too small: " + str(psize))

                print 'Sleep before next watch cycle ...'
            else:
                logLCD('NO faces detected!')
                
        else:
            print 'No Alarm ', distant,'cm' , 'trigger', trigger
            flushLCD('--- ' + str(distant) + ':'  + str(trigger))
            analogWrite(led,0)
            
        time.sleep(1)
        
    except KeyboardInterrupt:
        print 'DONE looking for faces ...'
        digitalWrite(buzzer_pin,0)
        break
    except TypeError:
        print "Error"
    except IOError:
        print "Error"



