#!/usr/bin/env python
# -*- coding: utf-8 -*-

# SmartLiving lightsensor experiment
# Important: before running this demo, make certain that grovepi & ATT_IOT
# are in the same directory as this script, or installed so that they are globally accessible

import grovepi                                     #provides pin support
import ATT_IOT as IOT 							   #provide cloud support
from time import sleep                             #pause the app

#set up the SmartLiving IoT platform
IOT.DeviceId   = "b8klgjaOEbLaCvncEX421VM"
IOT.ClientId   = "itirockz"
IOT.ClientKey  = "rcwenbi3ssg"

lightSensor = 0                                  #the PIN number of the lichtsensor, also used to construct a Unique assetID (DeviceID+nr)
LED = 3  

#set up the pins
grovepi.pinMode(lightSensor,"INPUT")
grovepi.pinMode( LED, 'output' )

#callback: handles values sent from the cloudapp to the device
def on_message( id, value ):
    print id
    if id.endswith( str( LED ) ):

        value = value.lower()

        if value == "true":
            print "ON"
            grovepi.digitalWrite( LED, 1 )

        if value == "false":
            print "OFF"
            grovepi.digitalWrite( LED, 0 )

    # ignore unkown ids and values                                                                                                                                               

#make certain that the device & it's features are defined in the cloudapp
IOT.on_message = on_message
IOT.connect()
IOT.addAsset(lightSensor, "lightSensor", "Light Sensor", False, "integer")
IOT.addAsset( LED, "LED", "Light Emitting Diode", True, "boolean" )
IOT.subscribe()              							#starts the bi-directional communication

#main loop: run as long as the device is turned on
while True:
    try:
        lightValue =  grovepi.analogRead(lightSensor)
        print( "LightSensor = " + str(lightValue))
        IOT.send(lightValue, lightSensor)
        sleep(5)

    except IOError:
        print ""
