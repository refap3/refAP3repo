#GroveStreams.com Raspberry Pi Python (version 3.2) Feed Example
#Demonstrates uploading Pi system metrics to GroveStreams using
# Python script and crontab. This python script will only upload the 
# current metrics one time. Schedule a linux crontab to upload the 
# metrics at a specified time interval.
 
#A full "how to" guide for this example can be found at:
# https://www.grovestreams.com/developers/getting_started_rpi.html
#It relies and the GroveStreams advanced feed PUT API which can be found here:
# https://www.grovestreams.com/developers/apibatchfeed.html#apu1b
 
#GroveStreams Setup:
# Sign Up for Free User Account - https://www.grovestreams.com
# Create a GroveStreams organization
# Enter the GroveStreams api key under "GroveStreams Settings" below  
#    (Can be retrieved from a GroveStreams organization:
#     click the Api Keys toolbar button,
#     select your Api Key, and click View Secret Key)

#Raspberry Pi Requirements
# Raspberry Pi with Ethernet Connection
# Raspberry Pi OS: Debian GNU/Linux 7 (wheezy)

import time
import json
import socket
import http.client
import gzip

from linux_metrics import cpu_stat 
from linux_metrics import disk_stat 
from linux_metrics import mem_stat 
from linux_metrics import net_stat 

class GroveStreams(object):
   
    # class constructor
    def __init__(self):

        #GroveStreams Settings
        self._apiKey = '038ac5fb-3c64-38ae-bcd2-a178d547a8a7'
        self._compress = True
        
        #The component ID is the IP address
        self._compId = socket.gethostbyname(socket.gethostname())
        
        #The name is from /etc/hostname
        self._compName = socket.gethostname() + ' (' + self._compId + ')';
        
        self._domain = 'www.grovestreams.com'
        self._baseUrl = '/api/feed'
        
        #Alternate API that will create streams on-the-fly based on the
        # template IDs within the URL.
        # self._baseUrl = '/api/feed?dtId=std&dsId=1minDbl'
    
    def run(self):

        print('GroveStreams starting sample reads')
        
        try:
            timer = time.time()
            
            #Get system metrics
            jobj = self.getMetrics()
            
            #Convert to JSON
            encoded_json = json.dumps(jobj)
        
            #Start timer 
            timer = time.time()
                
            #Upload metrics to GroveStreams
            result = self.sendToGs(encoded_json)
             
            #Stop timer    
            sendSamplesTime = time.time() - timer
            
            print('Upload time: %f seconds' % (sendSamplesTime))
        
        except Exception as e:
            print('Failure: ' + str(e))
        
        print('GroveStreams metrics upload complete')
        
        return  

    def getMetrics(self):
 
        now = int(time.time()) * 1000
        
        #JSON for GS Advanced Feed PUT API

        comp = {}
        comp['stream'] = []
        comp['componentId'] = self._compId
        comp['time'] = [now]
        comp['defaults'] = {}
        comp['defaults']['name'] = self._compName
        #Assign a component template for auto registration
        comp['compTmplId'] = 'rPi'
 
        
        #cpu streams
        cpu_pcts = cpu_stat.cpu_percents(1)
        stream = {}
        stream['streamId'] = 'cpu_user'
        stream['data'] = [cpu_pcts['user']]
        comp['stream'].append(stream)
                
        stream = {}
        stream['streamId'] = 'cpu_nice'
        stream['data'] = [cpu_pcts['nice']]
        comp['stream'].append(stream)   
        
        stream = {}
        stream['streamId'] = 'cpu_system'
        stream['data'] = [cpu_pcts['system']]
        comp['stream'].append(stream)      
        
        stream = {}
        stream['streamId'] = 'cpu_idle'
        stream['data'] = [cpu_pcts['idle']]
        comp['stream'].append(stream)
        
        stream = {}
        stream['streamId'] = 'cpu_iowait'
        stream['data'] = [cpu_pcts['iowait']]
        comp['stream'].append(stream)    
                
        load = cpu_stat.load_avg()
        stream = {}
        stream['streamId'] = 'cpu_avg1min'
        stream['data'] = [load[0]]
        comp['stream'].append(stream)  
        
        stream = {}
        stream['streamId'] = 'cpu_avg5min'
        stream['data'] = [load[1]]
        comp['stream'].append(stream)  
        
        stream = {}
        stream['streamId'] = 'cpu_avg15min'
        stream['data'] = [load[2]]
        comp['stream'].append(stream)  
        
        #processes
        stream = {}
        stream['streamId'] = 'procs_running'
        stream['data'] = [cpu_stat.procs_running()]
        comp['stream'].append(stream) 
        
        #memory
        mem = mem_stat.mem_stats()
        stream = {}
        stream['streamId'] = 'mem_active'
        stream['data'] = [mem[0]]
        comp['stream'].append(stream)       
                
        stream = {}
        stream['streamId'] = 'mem_total'
        stream['data'] = [mem[1]]
        comp['stream'].append(stream)  
        
        stream = {}
        stream['streamId'] = 'mem_cached'
        stream['data'] = [mem[2]]
        comp['stream'].append(stream)  
        
        stream = {}
        stream['streamId'] = 'mem_free'
        stream['data'] = [mem[3]]
        comp['stream'].append(stream)  
        
        stream = {}
        stream['streamId'] = 'swap_total'
        stream['data'] = [mem[4]]
        comp['stream'].append(stream)      
        
        stream = {}
        stream['streamId'] = 'swap_free'
        stream['data'] = [mem[5]]
        comp['stream'].append(stream)   
        
        #disk
        disk = disk_stat.disk_usage('/')
        stream = {}
        stream['streamId'] = 'disk_size'
        stream['data'] = [disk[1]]
        comp['stream'].append(stream)     
        
        stream = {}
        stream['streamId'] = 'disk_used'
        stream['data'] = [disk[2]]
        comp['stream'].append(stream)      
        
        #network
        net = net_stat.rx_tx_bytes('eth0')
        stream = {}
        stream['streamId'] = 'net_download'
        stream['data'] = [net[0]]
        comp['stream'].append(stream)       
        
        net = net_stat.rx_tx_bytes('eth0')
        stream = {}
        stream['streamId'] = 'net_upload'
        stream['data'] = [net[1]]
        comp['stream'].append(stream)      

        jobj = {}
        jobj['feed'] = {}
        jobj['feed']['component'] = []
        jobj['feed']['component'].append(comp)
        
        return jobj
        
    def sendToGs(self, json_encoded):
        #The GroveStreams API is based on REST
        
        if self._compress:
            body = gzip.compress(json_encoded.encode('utf-8'))
            print('Compressed feed ' + str(100*len(body) / len(json_encoded)) + '%')
            headers = {'Content-Encoding' : 'gzip' , 'Connection' : 'close', 'Content-type': 'application/json', 'Cookie' : 'api_key='+self._apiKey}
             
        else: 
            body = json_encoded
            headers = {'Connection' : 'close', 'Content-type': 'application/json', 'Cookie' : 'api_key='+self._apiKey}
                
        try:    
            conn = http.client.HTTPConnection(self._domain)  
           
            print('Uploading feed to: ' + self._domain + self._baseUrl)
            conn.request("PUT", self._baseUrl, body, headers)

            #Check for errors
            response = conn.getresponse()
            status = response.status
            
            if status != 200 and status != 201:
                try:
                    print('HTTP Failure Status: ' + str(status));
                    if (response.reason != None):
                        print('HTTP Failure Reason: ' + response.reason + '. body: ' + response.read().decode(encoding='UTF-8'))
                    else:
                        print('HTTP Failure Body: ' + response.read().decode(encoding='UTF-8'))
 
                except Exception as e:
                    print('HTTP Failure: ' + str(e))
                    
        finally:
            if conn != None:
                conn.close()
 
if __name__ == '__main__':
    
    GroveStreams = GroveStreams()
    
    GroveStreams.run()
    
    # Clean Up
    del GroveStreams
 
    # quit
    exit(0)  


