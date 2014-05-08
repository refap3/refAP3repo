using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Gsiot.Server;



//sample code for reverse HTTP via yaler ... Uses Netduino Plus 1 with 4.2 Firmware
//read sensor values 
//change actuators ....

//download stuff from here: http://www.gsiot.info/download/
//specifically the GSIOT.Server dll (referenced here )




namespace GSIOTSampleFW4._2
{
    public class Program
    {

        static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

        static OutputPort light = new OutputPort(Pins.GPIO_PIN_D13, false);
        static bool lOn = false; //keep the state in a static since reley usqage gives false read outs !!

        static OutputPort lowPort = new OutputPort(Pins.GPIO_PIN_A0, false);
        static AnalogInput voltagePort = new AnalogInput(Cpu.AnalogChannel.ANALOG_1);

        static OutputPort highPort = new OutputPort(Pins.GPIO_PIN_A2, true);

        public static void Main()
        {



            var t = new Thread(blink);
            t.Start();

            var webServer = new HttpServer();
            webServer.RelayDomain = "gsiot-bcjp-yj88";
            webServer.RelaySecretKey = "HDMvyM11hAu6H6cxIaT50dL9ALWc81MYB8H/UFhV";
            webServer.RequestRouting.Add("GET /hello", HandleHello);
            webServer.RequestRouting.Add("GET /temp", HandleTemp);
            webServer.RequestRouting.Add("GET /on", HandleOn);
            webServer.RequestRouting.Add("GET /off", HandleOff);
            webServer.RequestRouting.Add("GET /toggle", HandleToggle); 
            webServer.Run();


        }

        private static void blink()
        {
            while (true)
            {
                Thread.Sleep(2000);
                led.Write(!led.Read());
                //Debug.Print(voltagePort.Read().ToString());
            }
        }

        static void HandleHello ( RequestHandlerContext context)
        {

            string s = "<h1> hello webber " + DateTime.Now.ToString();

            context.SetResponse(s, "text/html");
        }

        static void HandleTemp(RequestHandlerContext context)
        {

            string s = "<h1>Temp " + voltagePort.Read().ToString();

            context.SetResponse(s, "text/html");
        }

        static void HandleOn(RequestHandlerContext context)
        {
            light.Write(true);
            string s = "<h1>ON " + DateTime.Now.ToString(); 

            context.SetResponse(s, "text/html");
        }
        static void HandleOff(RequestHandlerContext context)
        {
            light.Write(false);
            string s = "<h1>OFF " + DateTime.Now.ToString(); 

            context.SetResponse(s, "text/html");
        }
        static void HandleToggle(RequestHandlerContext context)
        {


            light.Write(!lOn); lOn = !lOn;
            string s = "<h1>Toggle -- was on: " +(!lOn).ToString() + " " + DateTime.Now.ToString();

            context.SetResponse(s, "text/html");
        }        

    }
}
