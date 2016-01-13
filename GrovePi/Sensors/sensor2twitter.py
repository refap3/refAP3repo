#!/usr/bin/python

# Tweet the temperature, light, and sound levels 

import time
import grovepi
import math
import tweepy  
import sys  
import datetime
  
# Connections
sound_sensor = 0        # port A0
light_sensor = 1        # port A1 
temperature_sensor = 4  # port D4
led = 3                 # port D3

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
st = datetime.datetime.fromtimestamp(ts).strftime('%Y-%m-%d %H:%M:%S')
api.update_status("Session start at: " + st)

print "Twitter Connected"

grovepi.pinMode(led,"OUTPUT")

last_sound = 0

while True:
    # Error handling in case of problems communicating with the GrovePi
    try:
        # Get value from temperature sensor
        [temp,humidity] = grovepi.dht(temperature_sensor,0)
        t=temp
        h=humidity


        # Get value from light sensor
        light_intensity = grovepi.analogRead(light_sensor)

        # Give PWM output to LED
        grovepi.analogWrite(led,light_intensity/4)

        # Get sound level
        sound_level = grovepi.analogRead(sound_sensor)
        if sound_level > 0:
            last_sound = sound_level

        # Post a tweet
        print            ("Temp: %.2f, Hum: %.2f, Light: %d, Sound: %d" %(t,h,light_intensity/10,last_sound))
        api.update_status("Temp: %.2f, Hum: %.2f, Light: %d, Sound: %d" %(t,h,light_intensity/10,last_sound))
        time.sleep(180)
    except IOError:
        print "Error"
    except:
        print "Twittwer exception"
