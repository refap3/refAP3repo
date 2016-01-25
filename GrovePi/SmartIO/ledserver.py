
import grovepi
import ATT_IOT as IOT
import time

LED = 3  

def on_message( id, value ):
    print id
    if id.endswith( str( LED ) ):

        value = value.lower()

        if value == "true":
            print "ON"
            grovepi.digitalWrite( LED, 1 )

        if value == "false":
            print "OFF"
            grovepi.digitalWrite( LED, 0 )

    # ignore unkown ids and values                                                                                                                                               

if __name__ == '__main__':

  grovepi.pinMode( LED, 'output' )

  IOT.DeviceId   = "b8klgjaOEbLaCvncEX421VM"
  IOT.ClientId   = "itirockz"
  IOT.ClientKey  = "rcwenbi3ssg"

  IOT.on_message = on_message

  IOT.connect()
  IOT.addAsset( LED, "LED", "Light Emitting Diode", True, "boolean" )
  IOT.subscribe()

  while True:
      time.sleep( .1 )

