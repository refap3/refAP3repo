# burn cookies on your capacity touch enabled smartphone 
# the LED resembled the realy frqcy 
# ALAS this does NOT reliably work!!

import time
import grovepi

# Connect the Rotary Angle Sensor to analog port A2
# potentiometer range is 0 to 1023 
potentiometer = 2


# Connect the LED to digital port D5
led = 5

# relay to D2
relay = 2

grovepi.pinMode(led,"OUTPUT")
grovepi.pinMode(relay,"OUTPUT")

time.sleep(1)
i = 0
relayState=True
potRead=0
potMax=10 # every this time read the poti 
while True:
    try:
        if potRead == 0: 
            # Read resistance from Potentiometer
            i = grovepi.analogRead(potentiometer)
            print i
            # Send PWM signal to LED
            grovepi.analogWrite(led,i/4)

        if potRead == potMax:
            potRead = 0 
        else:
            potRead += 1
        
        # handle relay
        relayState= not relayState
        grovepi.digitalWrite(relay,relayState)
        # dont go below 100hz
        if i<102:
            i=102
        time.sleep ((i+1)/1024.0)

    except IOError:
        print "Error"
