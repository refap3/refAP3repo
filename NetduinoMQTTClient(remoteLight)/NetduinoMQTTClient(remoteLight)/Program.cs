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
        private static string lastXivelyData = "" ;

        private static OutputPort remoteLight = new OutputPort(Pins.GPIO_PIN_D11, false);
        private static OutputPort debugLight = new OutputPort(Pins.ONBOARD_LED, false);
        private static OutputPort watchDogLight = new OutputPort(Pins.GPIO_PIN_D9, false);

        private static AnalogInput voltagePort = new AnalogInput(Pins.GPIO_PIN_A1);
        private static OutputPort lowPort = new OutputPort(Pins.GPIO_PIN_A0, false);
        private static OutputPort highPort = new OutputPort(Pins.GPIO_PIN_A2, true);



        public static void Main()
        {
            // connect to xively via  MQTT subscription ...
            ConnectSubscribe(APIKEY, FEEDID);

            voltagePort.SetRange(0, 9999); // set lumen sensitivity range 

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
            lastXivelyData=(msg.Split('\n')[1].Split(',')[2]); // in this case take 2nd datapoint and get value
            Debug.Print("xive: " + lastXivelyData);
            // since this repeatedly hangs try it on a new thread ....
            var t = new Thread(ActionForXivelyData);
            t.Start();
        }

        private static void ActionForXivelyData()
        {
            // do whatever you like with the new data, i.e. turn light on or off 
            int light = 0;
            try
            {
                light = int.Parse(lastXivelyData.Split('.')[0]); //throw away anything after "."
            }
            catch (Exception xx)
            {
                Debug.Print("Bad data exception: " + xx.ToString());
            }
            Debug.Print("lm Sensiti " + voltagePort.Read());
            remoteLight.Write(light < voltagePort.Read());
            debugLight.Write(remoteLight.Read());

        }
    }
}
