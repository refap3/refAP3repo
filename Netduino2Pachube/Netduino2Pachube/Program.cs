using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Text;

namespace Netduino2Pachube
{
    public class Program
    {
        static int tempr = 0; //simulated temp 
        const string NEWLINE = "\n";

        public static void Main()
        {
            // write your code here
            var interruptPort = new InterruptPort(Pins.ONBOARD_SW1, false, ResistorModes.Disabled, InterruptModes.InterruptEdgeBoth);
            interruptPort.OnInterrupt += new NativeEventHandler(interruptPort_OnInterrupt);
            //sleep for ever 
            while (true)
            {
                Thread.Sleep(1000); 
            }
        }

        static void interruptPort_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            // display IP address just to make sure .---
            var nwif = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];
            Debug.Print("IP: " + nwif.IPAddress.ToString());

            double tempMeas = tempr++;
            double humMeas = tempr++;
            double lightMeas = tempr++;

            SubmitToXively(tempMeas, humMeas, lightMeas);

        }

        private static void SubmitToXively(double tempMeas, double humMeas, double lightMeas)
        {
            // Note: the real API endpoint would be https://api.xively.com/v2/feeds/1934589243
            // dunno howto handle https though .. dunno if it would be supported on Spider 
            //

            const string APIKEY = "820f-wFPt2yWqCxSwk4t3gvP4F2SAKw4V2s3TEsycGJhVT0g";
            const string APIENDPOINT = "http://api.pachube.com/v2/feeds/1934589243";

            // deliver the  measurements to xively
            //
            string result = "Humidity, " + humMeas.ToString() + NEWLINE + "  Light, " + lightMeas.ToString() + NEWLINE + "  Temperature, " + tempMeas.ToString();


            Debug.Print("data: " + result);
            byte[] bytes = Encoding.UTF8.GetBytes(result);
            string strRequestUri = APIENDPOINT + ".csv";

            // add reference to system.http !!

            try
            {
                using (var request = (HttpWebRequest)WebRequest.Create(strRequestUri))
                {

                    request.Method = "PUT";
                    //add headers ...
                    request.ContentType = "text/csv";
                    request.ContentLength = bytes.Length;
                    request.Headers.Add("X-ApiKey", APIKEY);

                    //add content ...
                    var stream = request.GetRequestStream();
                    stream.Write(bytes, 0, bytes.Length);

                    //send 

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {

                        Debug.Print("Status: " + response.StatusCode);
                    }
                }
            }
            catch (Exception xx)
            {
                Debug.Print("Exception: " + xx.ToString());
            }
        }

    }
}
