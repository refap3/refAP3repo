#!/usr/bin/python 

from sense_hat import SenseHat
import time
import math
import requests
import json 
from random import randint
try:
    # For Python 3.0 and later
    from urllib.request import urlopen
except ImportError:
    # Fall back to Python 2's urllib2
    from urllib2 import urlopen



def randomsparkle(sense):
    x = randint(0, 7)
    y = randint(0, 7)
    r = randint(0, 255)
    g = randint(0, 255)
    b = randint(0, 255)
    sense.set_pixel(x, y, r, g, b)

def postEQ(quakstr):
    # note that the param value is encoded in url !
    url = 'https://dweet.io:443/dweet/for/stit17lastQ?stit17lastQ='+quakstr
    response = requests.post(url)

    print response
    data = json.loads(response.text)

    print  (data) 
    
def dweetEQ(quakstr):
    url = 'http://stit17.azurewebsites.net:80/api/alexa/lastquake?lastQuakeStrentgh='+quakstr
    response = requests.post(url)

    print response
    data = json.loads(response.text)

    print  (data) 

def get_jsonparsed_data(url):
    """
    Receive the content of ``url``, parse it as JSON and return the object.

    Parameters
    ----------
    url : str

    Returns
    -------
    dict
	
	Call like this: 
	-----------------
	j=get_jsonparsed_data(url)
	print j
	print j["Ort"]
	
    """
    response = urlopen(url)
    data = response.read().decode("utf-8")
    return json.loads(data)

def setup():
    time.sleep(0.1)

def loop():
    # DO FOREVER 
    while True:

        # get the run time parameters ..... we do this HERE in order to respond w/o restarts
        # minimum Quake Strength and Measure Time for quake  ...
        configBaseURL="http://stit17.azurewebsites.net:80/api/Configurations/"
        indexMINQUAKE=8
        indexMEASURETIME=9
        paramjson=get_jsonparsed_data(configBaseURL+str(indexMINQUAKE))
        LIMIT=int(paramjson["Value"]) # no quake below this !
        paramjson=get_jsonparsed_data(configBaseURL+str(indexMEASURETIME))
        MEASURE=int(paramjson["Value"]) #  # of secs to measure 
    
        # RANDOM Sparkle for 3 secs ...
        t_end=time.time() + 3 # run for x secs
        while time.time() < t_end:
            randomsparkle(sense)
            time.sleep(0.01)

        # BLANK screen for 1 secs ...
        t_end=time.time() + 1 # run for x secs
        while time.time() < t_end:
            pixels = [BLUE for i in range(64)]
            sense.set_pixels(pixels)
        
        # flash brief 
        pixels = [WHITE for i in range(64)]
        sense.set_pixels(pixels)
        time.sleep(0.5)

        # GO 
        pixels = [BLACK for i in range(64)]
        sense.set_pixels(pixels)
        quak=0.0
        t_end=time.time() + MEASURE # run for x secs
        while time.time() < t_end:
            x, y, z = sense.get_accelerometer_raw().values()
            x = round(x, 2)
            y = round(y, 2)
            z = round(z, 2)
            quak+=math.sqrt((0-x)**2+(0-y)**2+(1-z)**2)
            quakled=math.sqrt(quak)/30 * 64 
            pixels = [RED if i < quakled else BLACK for i in range(64)]
            sense.set_pixels(pixels)
            # print("x=%s, y=%s, z=%s" % (x, y, z))
            time.sleep(0.1)
        
        quak=math.sqrt(quak)
        quakstr=str(round(quak,1)).replace('.',',')
        
        if quak > LIMIT:
            print "you quaked: " + quakstr
            sense.show_message(str(int(round(quak,0))))
            postEQ(quakstr)
            dweetEQ(quakstr)
        else:
            print "NOPE only: " + quakstr
            sense.show_message("NOPE: " +str(int(round(quak,0))))


# REGION INIT ...        
print "STARTUP...................."
LIMIT = 6 # no quake below this !
sense = SenseHat()
RED = (255, 0, 0)
BLUE = ( 0, 0, 255)
BLACK = (0, 0, 0)
WHITE = (255,255,255)


print "DONE STARTUP...................."
          
if __name__ == '__main__':
    setup()
try:
    loop()
except KeyboardInterrupt: 
    print "Stopped"
finally:
    print "Exited"

