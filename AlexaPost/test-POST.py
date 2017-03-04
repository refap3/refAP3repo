#!/usr/bin/python 

from sense_hat import SenseHat

import requests
import json 
# note that the param value is encoded in url !
url = 'http://stit17.azurewebsites.net:80/api/alexa/lastquake?lastQuakeStrentgh=9'
response = requests.post(url)

print response
data = json.loads(response.text)

print  (data) 

