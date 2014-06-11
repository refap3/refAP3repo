# good commands are: on off tg (test)
# note: sudo NOT needed in this case !!
#
sudo su <<HERE
case "$1" in 

on) 
echo 17 > /sys/class/gpio/export
echo out   > /sys/class/gpio/gpio17/direction 
echo 0   > /sys/class/gpio/gpio17/value
echo "EXECUTED command on"
;;

off) 
echo 17 > /sys/class/gpio/export
echo out   > /sys/class/gpio/gpio17/direction 
echo 1   > /sys/class/gpio/gpio17/value
echo "EXECUTED command off"
;;

tg) 
echo "WILL execute command tg ..."
echo 17 > /sys/class/gpio/export
echo out   > /sys/class/gpio/gpio17/direction 
echo 0   > /sys/class/gpio/gpio17/value
sleep 8 
echo 1   > /sys/class/gpio/gpio17/value
echo "EXECUTED command tg"
;;
*) echo "some dumb verb -- use on off tg"
   echo $1
;;
esac

exit 

HERE
