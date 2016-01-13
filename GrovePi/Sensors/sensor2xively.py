#!/usr/bin/python

# xively the temperature, light, and sound levels 
# & dump to LCD display as well!
#   ----------------------------

import time
import xively 
import subprocess
import requests
import math
import sys  
import datetime

import grovepi
from grove_rgb_lcd import *

FEED_ID = "1912866555"
API_KEY = "VXsmVTemNsreZfSUohR2i8x7D9c5IYq9nrfv4W78TulKVd56"

# initialize api client
api = xively.XivelyAPIClient(API_KEY)

setRGB(0,128,64)
setRGB(0,255,0)
setText("Connected to xively ... ")
time.sleep(3)

# function to return a datastream object. This either creates a new datastream,
# or returns an existing one
def get_datastream(feed,label,tag):
  try:
    datastream = feed.datastreams.get(label)
    return datastream
  except:
    datastream = feed.datastreams.create(label, tags=tag)
    return datastream

# Connections
sound_sensor = 0        # port A0
light_sensor = 1        # port A1 
temperature_sensor = 4  # port D4
led = 3                 # port D3

ts = time.time()
st = datetime.datetime.fromtimestamp(ts).strftime('%Y-%m-%d %H:%M:%S')
grovepi.pinMode(led,"OUTPUT")

last_sound = 0

feed = api.feeds.get(FEED_ID)

dsTEMP= get_datastream(feed,"GrovePiTemp","temperature")
dsTEMP.max_value = None
dsTEMP.min_value = None

dsHUM= get_datastream(feed,"GrovePiHum","humidity")
dsHUM.max_value = None
dsHUM.min_value = None

dsSND= get_datastream(feed,"GrovePiSound","db")
dsSND.max_value = None
dsSND.min_value = None

dsLIGHT= get_datastream(feed,"GrovePiLight","cnd")
dsLIGHT.max_value = None
dsLIGHT.min_value = None

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

        # dump values to LCD 
        setRGB(0,128,64)
        setRGB(0,255,0)
        setText("T:%.2f H:%.2f\nL:%d    S:%d" %(t,h,light_intensity/10,last_sound))

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

        print ("Temp: %.2f, Hum: %.2f, Light: %d, Sound: %d" %(t,h,light_intensity/10,last_sound))
        time.sleep(10)
    except TypeError:
        print "TypeError"
    except IOError:
        print "Error"
