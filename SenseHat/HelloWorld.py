#!/usr/bin/python 

from sense_hat import SenseHat
sense=SenseHat()

sense.show_message("Hello Sense Hat!",scroll_speed=0.05, text_colour=[255,255,0], back_colour=[0,0,255])

