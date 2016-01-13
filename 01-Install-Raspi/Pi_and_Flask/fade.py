import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)
GPIO.setup(25, GPIO.OUT)

p = GPIO.PWM(25, 50) 
p.start(0) 

while True:
   for dc in range(0, 100, 5): 
      p.ChangeDutyCycle(dc) 
      time.sleep(0.05)
   for dc in range(100, 0, -5): 
      p.ChangeDutyCycle(dc)
      time.sleep(0.05)