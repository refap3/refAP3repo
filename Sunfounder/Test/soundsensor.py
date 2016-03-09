#!/usr/bin/env python
import PCF8591 as ADC
import time
def setup():
    ADC.setup(0x48)
 
def loop():
    count = 0
    while True:
        tmp = ADC.read(0)
        print tmp
        time.sleep (0.4)
        if tmp < 50:
            count += 1
            print "Voice In!!  ", count
 
def destroy():
    ADC.write(0)
 
if __name__ == "__main__":
    try:
        setup()
        loop()
    except KeyboardInterrupt:
        destroy()