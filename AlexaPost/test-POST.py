#!/usr/bin/python 

from sense_hat import SenseHat
# sense=SenseHat()

# tets POST to Alexa WS from python: 

# http://stit17.azurewebsites.net:80/api/alexa/lastquake?lastQuakeStrentgh=8

# >>> payload = {'key1': 'value1', 'key2': 'value2'}
# >>> r = requests.post("http://httpbin.org/post", data=payload)


import requests
import json 
url = 'http://stit17.azurewebsites.net:80/api/alexa/lastquake'
payload = {'lastQuakeStrentgh': '7'}
#response = requests.post(url, data=payload)
url = 'http://stit17.azurewebsites.net:80/api/alexa/lastquake?lastQuakeStrentgh=9'
response = requests.post(url)

print response
data = json.loads(response.text)

print  (data) 

