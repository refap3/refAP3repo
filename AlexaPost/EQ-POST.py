#!/usr/bin/python 

from sense_hat import SenseHat
import time
import math
import requests
import json 

sense = SenseHat()

def postEQ(quakstr):
    # note that the param value is encoded in url !
    url = 'http://stit17.azurewebsites.net:80/api/alexa/lastquake?lastQuakeStrentgh='+quakstr
    response = requests.post(url)

    print response
    data = json.loads(response.text)

    print  (data) 


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
    quakstr=str(round(quak,1)).replace('.',',')
    print "you quaked: " + quakstr
    postEQ(quakstr)

if __name__ == '__main__':
    setup()
try:
    loop()
except KeyboardInterrupt: 
    print "Stopped"
finally:
    print "Exited"
	
	



