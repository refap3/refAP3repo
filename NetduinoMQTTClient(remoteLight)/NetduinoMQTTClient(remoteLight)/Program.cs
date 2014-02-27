using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using uPLibrary.Networking.M2Mqtt;
using System.Text;

namespace NetduinoMQTTClient_remoteLight_
{

    // access xiveley datastreams and turn on/off light 
    //

    public class Program
    {
        private static string APIKEY = "820f-wFPt2yWqCxSwk4t3gvP4F2SAKw4V2s3TEsycGJhVT0g";
        private static string FEEDID = "/v2/feeds/1934589243.csv";
        private static string lastXivelyData2 = "";
        private static string lastXivelyData3 = "";

        private static OutputPort remoteLight = new OutputPort(Pins.GPIO_PIN_D11, false);
        private static OutputPort debugLight = new OutputPort(Pins.ONBOARD_LED, false);
        private static OutputPort watchDogLight = new OutputPort(Pins.GPIO_PIN_D9, false);




        public static void Main()
        {
            // connect to xively via  MQTT subscription ...
            ConnectSubscribe(APIKEY, FEEDID);

            //run watchdog on seperate thread ...
            var t = new Thread(DoWatchDogLight);
            t.Start();
        }

        private static void DoWatchDogLight()
        {
            while (true)
            {
                watchDogLight.Write(!watchDogLight.Read());
                Thread.Sleep(800);
            }
        }

        private static void ConnectSubscribe(string APIKEY, string FEEDID)
        {
            var ip = System.Net.IPAddress.Parse("216.52.233.120"); //this is api.xively.com ...

            var mqc = new MqttClient(ip);
            mqc.Connect(APIKEY);

            byte[] qosleve = new byte[1];
            qosleve[0] = 0;
            mqc.Subscribe(new string[] { APIKEY + FEEDID }, qosleve);

            mqc.MqttMsgPublishReceived += new MqttClient.MqttMsgPublishEventHandler(mqc_MqttMsgPublishReceived);
        }

        static void mqc_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            string msg = new string(Encoding.UTF8.GetChars(e.Message));
            // here we take data [2] (Light) and [3] (poti)
            lastXivelyData2 = (msg.Split('\n')[2].Split(',')[2]); // in this case take 2nd datapoint and get value
            lastXivelyData3 = (msg.Split('\n')[3].Split(',')[2]); // in this case take 3rd datapoint and get value
            Debug.Print("xive light: " + lastXivelyData2);
            Debug.Print("xive thresh: " + lastXivelyData3);
            // since this repeatedly hangs try it on a new thread ....
            var t = new Thread(ActionForXivelyData);
            t.Start();
        }

        private static void ActionForXivelyData()
        {
            // do whatever you like with the new data, i.e. turn light on or off 
            int light = 0; double thresh = 0; 
            try
            {
                light = int.Parse(lastXivelyData2.Split('.')[0]); //throw away anything after "."
                thresh = double.Parse(lastXivelyData3)*10000 ; // poti Range is 0 to 1 so make this 0 to 10000!
            }
            catch (Exception xx)
            {
                Debug.Print("Bad data exception: " + xx.ToString());
            }
            remoteLight.Write(light < thresh);
            debugLight.Write(remoteLight.Read());
            Debug.Print("l: " + light + "t: " + thresh);

        }
    }
}
