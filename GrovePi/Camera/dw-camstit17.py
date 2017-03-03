#!/usr/bin/python 

# bloody dweepy gets connection time out so this MUST be called in a loop by wrap_call shell script 
# ignoring the exception WONT help !
import dweepy 
import camstit17

try:
	thing = 'stit17tweet'
	print 'Subscribe dweet.io for thing: ' + thing
	for dweet in dweepy.listen_for_dweets_from(thing):
		print dweet
		camstit17.work()

		
except KeyboardInterrupt: 
	pass
		
# except Exception:
	# print "Unexpected error:"
	# pass

