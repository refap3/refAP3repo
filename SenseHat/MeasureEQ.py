#!/usr/bin/python 

from sense_hat import SenseHat
import time
import math
import locale
# locale.setlocale(locale.LC_ALL,'de_DE')

sense = SenseHat()



def setup():
    time.sleep(0.1)
def loop():
    quak=0.0
    t_end=time.time() + 3 # run for x secs
    while time.time() < t_end:
        x, y, z = sense.get_accelerometer_raw().values()
        x = round(x, 2)
        y = round(y, 2)
        z = round(z, 2)
        quak+=math.sqrt((0-x)**2+(0-y)**2+(1-z)**2)
        print("x=%s, y=%s, z=%s" % (x, y, z))
        time.sleep(0.1)
    quak=math.sqrt(quak)
    print "you quaked: " + str(round(quak,1)).replace('.',',')

if __name__ == '__main__':
    setup()
try:
    loop()
except KeyboardInterrupt: 
    print "Stopped"
finally:
    print "Exited"
	
		   
