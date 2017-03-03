#!/usr/bin/python 

# dweepy test SUBSCRIBE do NOT use https when testing this via swagger !
import dweepy 

try:
	thing = 'stit17tweet'
	print 'Subscribe dweet.io for thing: ' + thing
	for dweet in dweepy.listen_for_dweets_from(thing):
		print dweet

		
except KeyboardInterrupt: 
	pass
		
except Exception:
	print "Unexpected error:"
	pass

