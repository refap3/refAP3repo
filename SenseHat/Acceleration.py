#!/usr/bin/python 

from sense_hat import SenseHat
import time

sense = SenseHat()

while True:
  x, y, z = sense.get_accelerometer_raw().values()

  x = round(x, 2)
  y = round(y, 2)
  z = round(z, 2)

  print("x=%s, y=%s, z=%s" % (x, y, z))
  time.sleep(0.1)
