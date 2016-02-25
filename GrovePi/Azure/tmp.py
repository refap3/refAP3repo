import json

data =  { 'a':'A', 'b':(2, 4), 'c':3.0 } 

print 'DATA1:', data
print 'DATA2:', repr(data)

data_string = json.dumps(data)
print 'JSON:', data_string

while True:
    try:
        data =  { 'a':'xxx', 'b':(2, 4), 'c':3.0 } 
        print data
    except IOError:
        print "IOError"