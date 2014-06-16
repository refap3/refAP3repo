## this is the all script for camera install on python
##
##
## functions are: 
## ctl clear timelapse stuff
## ltl list timelapse directory 
## stl start timelapse 
## sca start adafruit camera module 
## sp show pythons 
## kp kill pythons (for above timelapse and adafruit)
##

echo "Welcome to the All camera Shell"
case "$1" in 

ctl)
sudo rm -R /home/timelapse/*
echo "Executed CLEAR timelapse"
;;

ltl)

ls -R /home/timelapse
echo "Executed LIST timelapse"
;;

stl)

cd /home/pi
sudo su <<sd
python raspiLapseCam.py &
sd

echo "Executed START timelapse"
;;

sca)
cd /home/pi/adafruit-pi-cam-master/
sudo python cam.py &
echo "Executed START adafruit camera"
;;

sp)
sudo su <<sd
ps -LA |grep python
sd
echo "Executed SHOW pythons" 
;;

kp)
sudo su <<sd
killall -9 python
sd
echo "Executed KILL pythons"
;;

*)

echo "some dumb verb!! use ctl ltl stl sca sp kp ..."
echo $1
;;

esac

exit


