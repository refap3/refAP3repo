import ATT_IOT as IOT 							   #provide cloud support
from time import sleep                             #pause the app

#set up the SmartLiving IoT platform
IOT.DeviceId   = "b8klgjaOEbLaCvncEX421VM"
IOT.ClientId   = "itirockz"
IOT.ClientKey  = "rcwenbi3ssg"

lightSensor = 0                                  #the PIN number of the lichtsensor, also used to construct a Unique assetID (DeviceID+nr)
LED = 3  


#callback: handles values sent from the cloudapp to the device
def on_message( id, value ):
    print id
    if id.endswith( str( LED ) ):

        value = value.lower()

        if value == "true":
            print "ON"

        if value == "false":
            print "OFF"

    # ignore unkown ids and values                                                                                                                                               

#make certain that the device & it's features are defined in the cloudapp
IOT.on_message = on_message
IOT.connect()
IOT.addAsset(lightSensor, "lightSensor", "Light Sensor", False, "integer")
IOT.addAsset( LED, "LED", "Light Emitting Diode", True, "boolean" )
IOT.subscribe()              							#starts the bi-directional communication

#main loop: run as long as the device is turned on
lightValue = 0 
while True:
    try:
        lightValue += 1
        print( "LightSensor = " + str(lightValue))
        IOT.send(lightValue, lightSensor)
        sleep(5)

    except IOError:
        print ""
