import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)
GPIO.setup(24,GPIO.IN)
GPIO.setup(25,GPIO.OUT)



while True: 
	inputValue=GPIO.input(24)
	if (inputValue == True):
		GPIO.output(25,GPIO.HIGH)
		time.sleep(0.3)
	GPIO.output(25,GPIO.LOW)
	time.sleep(0.3)


