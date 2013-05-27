using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace NetduinoPlusApplication1
{
    public class Program
    {
        private static OutputPort ledPort = new OutputPort(Pins.ONBOARD_LED, false);
        private static OutputPort realLedPort = new OutputPort(Pins.GPIO_PIN_D13, false);
        private static InputPort realSwitchPort = new InputPort(Pins.GPIO_PIN_D0, false, Port.ResistorMode.Disabled);
        private static InputPort realSwitchPort1 = new InputPort(Pins.GPIO_PIN_D1, false, Port.ResistorMode.Disabled);
        private static InputPort realSwitchPort2 = new InputPort(Pins.GPIO_PIN_D2, false, Port.ResistorMode.Disabled);

        private static InputPort switchPort;
        private static InterruptPort interruptPort;
        private static int sleepTime = 0;

        public static void Main()
        {
            var dummyTest = true;
            //Connect real Input Ports to ground !

            //3 Examples: 
            //    1. "normal" 
            //    2. interrupt driven 
            //    3. PWM driven on port D5 !!

            var runSample = 1;
            switch (runSample)
            {   // "normal" 
                case 1:
                    switchPort = new InputPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled);
                    while (dummyTest)
                    {
                        if (!switchPort.Read() || !realSwitchPort.Read() || !realSwitchPort1.Read() || !realSwitchPort2.Read())
                        {
                            ledPort.Write(true);
                            realLedPort.Write(true);
                            sleepTime = 500;
                            if (!realSwitchPort1.Read()) sleepTime = 300;
                            if (!realSwitchPort2.Read()) sleepTime = 100;
                        }
                        Thread.Sleep(sleepTime);
                        ledPort.Write(false);
                        realLedPort.Write(false);
                        Thread.Sleep(sleepTime);
                    }
                    break;
                case 2:
                    // Interrupt Sample: 
                    interruptPort = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
                    interruptPort.OnInterrupt += new NativeEventHandler(interruptPort_OnInterrupt);
                    while (dummyTest)
                    {
                    }
                    break;
                case 3:
                    // use PWM Port ..
                    var realLedPortPWM = new PWM(Pins.GPIO_PIN_D5);
                    uint outPWM = 0; bool up = true;
                    while (dummyTest)
                    {
                        if (up)
                        {
                            outPWM++;
                            if (outPWM >= 100)
                            {
                                up = false;
                            }
                        }
                        else
                        {
                            outPWM--;
                            if (outPWM <= 0)
                            {
                                up = true;
                            }
                        }
                        realLedPortPWM.SetDutyCycle(outPWM);
                        Thread.Sleep(8);
                    }
                    break;
                default:
                    Debug.Print("This is no choice -- you are doomed!");
                    break;
            }
        }
        static void interruptPort_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            var portData = (data2 == 0);
            ledPort.Write(portData);
            realLedPort.Write(portData);
        }
    }
}
