#!/usr/bin/python 

from sense_hat import SenseHat
import time
import math
import requests
import json 
from random import randint

LIMIT = 6 # no quake below this !
sense = SenseHat()
RED = (255, 0, 0)
BLUE = ( 0, 0, 255)
BLACK = (0, 0, 0)
WHITE = (255,255,255)

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
    

def setup():
    time.sleep(0.1)
def loop():
    # DO FOREVER 
    while True:
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
        t_end=time.time() + 3 # run for x secs
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
        quakstr1=str(round(quak,1))
        quakstr=quakstr1.replace('.',',')
        
        if quak > LIMIT:
            print "you quaked: " + quakstr
            sense.show_message(str(int(round(quak,0))))
            postEQ(quakstr)
            dweetEQ(quakstr1)
        else:
            print "NOPE only: " + quakstr
            sense.show_message("NOPE: " +str(int(round(quak,0))))
    
if __name__ == '__main__':
    setup()
try:
    loop()
except KeyboardInterrupt: 
    print "Stopped"
finally:
    print "Exited"

