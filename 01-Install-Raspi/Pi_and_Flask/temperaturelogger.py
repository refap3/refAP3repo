
import os
import xively
import subprocess
import time
import datetime
import requests

FEED_ID = "1375410327"
API_KEY = "vyvi4xDLkjCQLYwBoAE8wIyFeV0ENoqpFGqCl5Kv35aIFTIK"

# initialize api client
api = xively.XivelyAPIClient(API_KEY)

# function to read the temperature from ds18b20 temperature sensor on i2c
def read_temperature():
   tempfile = open("/sys/bus/w1/devices/28-0000057bc88f/w1_slave")
   thetext = tempfile.read()
   tempfile.close()
   tempdata = thetext.split("\n")[1].split(" ")[9]
   temperature = float(tempdata[2:])
   temperature = temperature / 1000
   return temperature

# function to return a datastream object. This either creates a new datastream,
# or returns an existing one
def get_datastream(feed):
  try:
    datastream = feed.datastreams.get("TemperatureSensor")
    return datastream
  except:
    datastream = feed.datastreams.create("TemperatureSensor", tags="temperature")
    return datastream

# main program entry point - runs continuously updating our datastream with the
# latest temperature reading
def run():
  feed = api.feeds.get(FEED_ID)

  datastream = get_datastream(feed)
  datastream.max_value = None
  datastream.min_value = None

  while True:
    degreesCelcius = read_temperature()
    datastream.current_value = degreesCelcius
    datastream.at = datetime.datetime.utcnow()
    try:
      datastream.update()
    except requests.HTTPError as e:
      print "HTTPError({0}): {1}".format(e.errno, e.strerror)

    time.sleep(10)

run()