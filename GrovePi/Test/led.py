#!/usr/bin/env python

import time
from grovepi import *

# Connect the Grove LED to digital port D1 - D4

time.sleep(1)
print "THE FAT Part within the LED body is the cathode (-)!" 
print ""
print ""
print "YOU MUST turn the brightness up to 100 %!" 
print "Connect the LEDs to the ports D2 to D4!" 

while True:
    try:
        for led in range (2,5):
            pinMode(led,"OUTPUT")
            digitalWrite(led,1)		# Send HIGH to switch on LED
            time.sleep(0.2)
            digitalWrite(led,0)		
            time.sleep(0.2)

    except KeyboardInterrupt:	# Turn LED off before stopping
        digitalWrite(led,0)
        break
    except IOError:				# Print "Error" if communication error encountered
        print ("Error")
