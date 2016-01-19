# run this from X environment (rdp)

from SimpleCV import Image, Display
from time import sleep

myDisplay=Display()
rImage=Image("test.jpg")
rImage.save(myDisplay)

while not myDisplay.isDone():
	sleep (0.1)