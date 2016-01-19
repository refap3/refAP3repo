# capture from raspicam and display frames ...

from SimpleCV import Camera, Display
from time import sleep

myCamera=Camera(0)
myDisplay=Display(resolution=(1024,768))

while not myDisplay.isDone():
	myCamera.getImage().save(myDisplay)
	sleep(0.01)