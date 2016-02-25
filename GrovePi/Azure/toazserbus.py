import json
import datetime
from grovepi import *
from azure.servicebus import ServiceBusService


dht_sensor_port = 6             # Connect the DHt sensor to this port 
dht_sensor_type = 0             # change this depending on your sensor type - see header comment
potentiometer=0                 # test device on Analog Port i.e. poti ...


sbs = ServiceBusService(service_namespace='STITlndf-ns', shared_access_key_name='sendPOLICY', shared_access_key_value='Yil7vtCJx1HwxKXq7F3xDN+sbMUT4nud6QRRqItuxL4=')

while True:
    try:
        [ temp,hum ] = dht(dht_sensor_port,dht_sensor_type)     #Get the temperature and Humidity from the DHT sensor
        if not math.isnan(temp):
            if not math.isnan(hum):
                #itemp=analogRead(potentiometer)
                #temp= itemp
                #hum=itemp
                t = str(temp)
                h = str(hum)
                jsn = {'DeviceId': 'room1', 'Temperature': t, 'Humidity': h}
                print jsn, ' ', datetime.datetime.now()
                sbs.send_event('stitlndf',json.dumps(jsn) )
        
    except (IOError,TypeError) as e:
        print 'Error'

