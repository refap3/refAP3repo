#!/usr/bin/python

# same as blink.py BUT using board mode instead of BCM
# GPIO 25 in BCM is 22 in BOARD mode

import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)
GPIO.setup(22,GPIO.OUT)
GPIO.output(22,GPIO.HIGH) # means OFF when HIGH signal
 
while True: 
	GPIO.output(22,GPIO.HIGH)
	time.sleep(0.1)
	GPIO.output(22,GPIO.LOW)
	time.sleep(0.1)

