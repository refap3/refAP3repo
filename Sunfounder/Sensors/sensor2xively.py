#!/usr/bin/python

# xively the temperature, light, and sound levels 
# & dump to LCD display as well!
# this is the SUNFOUNDER version !!
#   ----------------------------

import time
import xively 
import subprocess
import requests
import math
import sys  
import datetime

import Adafruit_DHT as DHT
import RPi.GPIO as GPIO
import PCF8591 as ADC
 


FEED_ID = "1912866555"
API_KEY = "VXsmVTemNsreZfSUohR2i8x7D9c5IYq9nrfv4W78TulKVd56"

# initialize api client
api = xively.XivelyAPIClient(API_KEY)

#setRGB(0,128,64)
#setRGB(0,255,0)
#setText("Connected to xively ... ")
print 'Connected to xively ...'
time.sleep(1)

# function to return a datastream object. This either creates a new datastream,
# or returns an existing one
def get_datastream(feed,label,tag):
  try:
    datastream = feed.datastreams.get(label)
    return datastream
  except:
    datastream = feed.datastreams.create(label, tags=tag)
    return datastream

feed = api.feeds.get(FEED_ID)

dsTEMP= get_datastream(feed,"SunfounderTemp","temperature")
dsTEMP.max_value = None
dsTEMP.min_value = None

dsHUM= get_datastream(feed,"SunfounderHum","humidity")
dsHUM.max_value = None
dsHUM.min_value = None

dsSND= get_datastream(feed,"SunfounderSound","db")
dsSND.max_value = None
dsSND.min_value = None

dsLIGHT= get_datastream(feed,"SunfounderLight","cnd")
dsLIGHT.max_value = None
dsLIGHT.min_value = None

# connections 
Sensor = 11
humiture = 17
lightSensor=1   # Ananlog In 1
soundSensor=0   # Ananlog In 0

#setup 
ADC.setup(0x48)

while True:
    # Error handling in case of problems communicating with the Sunfounder
    try:
        # Get value from temperature sensor
        [humidity,temp] = DHT.read_retry(Sensor, humiture)
        t=temp
        h=humidity

        # Get value from light sensor
        light_intensity = ADC.read(lightSensor)

        # Get sound level
        sound_level = ADC.read(soundSensor)

        # get seconds part of time 
        ts = time.time()
        sec = datetime.datetime.fromtimestamp(ts).strftime('%S')

        dsTEMP.current_value = t
        dsTEMP.at = datetime.datetime.utcnow()

        dsHUM.current_value = h
        dsHUM.at = datetime.datetime.utcnow()

        dsSND.current_value = sound_level
        dsSND.at = datetime.datetime.utcnow()

        dsLIGHT.current_value = light_intensity
        dsLIGHT.at = datetime.datetime.utcnow()

        try:
            dsTEMP.update()
            dsHUM.update()
            dsSND.update()
            dsLIGHT.update()

        except requests.HTTPError as e:
            print "HTTPError({0}): {1}".format(e.errno, e.strerror)

        print ("Temp: %.2f, Hum: %.2f, Light: %d, Sound: %d" %(t,h,light_intensity,sound_level))
        time.sleep(3)
    except TypeError:
        print "TypeError"
    except IOError:
        print "Error"
