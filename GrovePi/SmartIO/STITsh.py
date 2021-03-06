#!/usr/bin/env python
# -*- coding: utf-8 -*-

# STIT smart home demo ...
import grovepi                                     #provides pin support
import ATT_IOT as IOT 							   #provide cloud support
from time import sleep                             #pause the app

#set up the SmartLiving IoT platform
IOT.DeviceId   = "OF0TwpmI6rqhHjO8lrZp9LF"
IOT.ClientId   = "itirockz"
IOT.ClientKey  = "rcwenbi3ssg"

# devices used: 
lightRELAY=3    # connect relay to D3
heaterRELAY=4   # connect relay to D4
tempSENSOR=6    # connect temp sensor to D6
humSENSOR=tempSENSOR+1 # FAKE humidity sensor 

#set up the pins
grovepi.pinMode( tempSENSOR,"INPUT")
grovepi.pinMode( lightRELAY, "OUTPUT" )
grovepi.pinMode( heaterRELAY, "OUTPUT" )

#callback: handles values sent from the cloudapp to the device
def on_message( id, value ):
    print id
    if id.endswith( str( lightRELAY ) ):
        value = value.lower()
        if value == "true":
            print "l-ON"
            grovepi.digitalWrite( lightRELAY, 1 )
        if value == "false":
            print "l-OFF"
            grovepi.digitalWrite( lightRELAY, 0 )
    if id.endswith( str( heaterRELAY ) ):
        value = value.lower()
        if value == "true":
            print "h-ON"
            grovepi.digitalWrite( heaterRELAY, 1 )
        if value == "false":
            print "h-OFF"
            grovepi.digitalWrite( heaterRELAY, 0 )

    # ignore unkown ids and values                                                                                                                                               

#make certain that the device & it's features are defined in the cloudapp
IOT.on_message = on_message
IOT.connect()
IOT.addAsset(tempSENSOR, "tempSENSOR", "Temperature Sensor", False, "float")
IOT.addAsset(humSENSOR, "humSENSOR", "Humidity Sensor", False, "float")
IOT.addAsset( lightRELAY, "lightRELAY", "Light Switch", True, "boolean" )
IOT.addAsset( heaterRELAY, "heaterRELAY", "Heater Switch", True, "boolean" )
IOT.subscribe()              							#starts the bi-directional communication

#main loop: run as long as the device is turned on
while True:
    try:
        [temp, hum]=grovepi.dht(tempSENSOR,0)
        print( "t=",temp," h=",hum)
        IOT.send(temp, tempSENSOR)
        IOT.send(hum, humSENSOR)
        sleep(1)

    except IOError:
        print ""
    except TypeError:
        print ""
