#!/usr/bin/python

# pot_repeat.py - continuously measure resistance of a potentiometer
# (c) BotBook.com - Karvinen, Karvinen, Valtokari
# from the book getting started with sensors

# read and convert values between 0 and 1023 (10 bits)

import botbook_mcp3002 as mcp	# <1>
import time	# <2>

while(True):	# <3>
	x = mcp.readAnalog(0,0)	# <4>
	print(x)	# <5>
	time.sleep(0.5)	# seconds	# <6>

