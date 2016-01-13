#!/usr/bin/python

# Dump the temperature, light, and sound levels to console  AND LCD display !

import time
import grovepi
from grove_rgb_lcd import *
import math
import sys  
import datetime
  
# Connections
sound_sensor = 0        # port A0
light_sensor = 1        # port A1 
temperature_sensor = 4  # port D4
led = 3                 # port D3

ts = time.time()
st = datetime.datetime.fromtimestamp(ts).strftime('%Y-%m-%d %H:%M:%S')
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

        print ("Temp: %.2f, Hum: %.2f, Light: %d, Sound: %d" %(t,h,light_intensity/10,last_sound))


        # get seconds part of time 
        ts = time.time()
        sec = datetime.datetime.fromtimestamp(ts).strftime('%S')

        # dump values to LCD 
        setRGB(0,128,64)
        setRGB(0,255,0)
        setText("T:%.2f H:%.2f\nL:%d   S:%d %ss" %(t,h,light_intensity/10,last_sound,sec))

        time.sleep(3)
    except IOError:
        print "Error is LCD connected?"
