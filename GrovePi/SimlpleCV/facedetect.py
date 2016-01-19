# face detection on raspicam  ...

from SimpleCV import Camera, Display
from time import sleep

myCamera=Camera(0)
myDisplay=Display(resolution=(640,480))

while not myDisplay.isDone():
	frame=myCamera.getImage()
	faces=frame.findHaarFeatures('face')
	if faces:
		fct=0
		for face in faces:
			fct+=1
			print "face " + str(fct) + " at: " + str(face.coordinates())
			myFace=face.crop()  # last face seen 
			myFace.save(myDisplay)
			sleep(0.5)
			
	else:
		print "no face!"
		
