/*
 ThingSpeak Client
 This program allows you to update a ThingSpeak Channel via the ThingSpeak API using a Netduino Plus
 
 Getting Started with ThingSpeak:
 
    * Sign Up for New User Account - https://www.thingspeak.com/users/new
    * Create a New Channel by selecting Channels and then Create New Channel
    * Enter the Write API Key in this program under "ThingSpeak Settings"
  
 @created: January 26, 2011
 @updated: 
 
 @tutorial: http://community.thingspeak.com/tutorials/netduino/create-your-own-web-of-things-using-the-netduino-plus-and-thingspeak/
  
 @copyright (c) 2011 Hans Scharler (http://www.iamshadowlord.com)
 @licence: Creative Commons, Attribution-Share Alike 3.0 United States, http://creativecommons.org/licenses/by-sa/3.0/us/deed.en
  
 */

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Socket = System.Net.Sockets.Socket;

namespace ThingSpeakClient
{
    public class Program
    {

        //ThingSpeak Settings
        const string writeAPIKey = "MFPR39XBPL2DVBPB"; // Write API Key for a ThingSpeak Channel
        const string thingTweetAPIkey = "XW6MJIB1LM7BFFQ5";             // Thing Tweet API key 

        static string tsIP = "184.106.153.149";     // IP Address for the ThingSpeak API
        static Int32 tsPort = 80;                   // Port Number for ThingSpeak
        const int updateInterval = 5000;           // Time interval in milliseconds to update ThingSpeak 

        static int analogReading;

        static OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        static InterruptPort button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh); //send tweet on button press 
        private static OutputPort lowPort = new OutputPort(Pins.GPIO_PIN_A0, false);
        private static OutputPort highPort = new OutputPort(Pins.GPIO_PIN_A2, true);

        public static void Main()
        {
            AnalogInput A0 = new AnalogInput(Pins.GPIO_PIN_A1);
            A0.SetRange(0, 9999); // Set up analogue range 
            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);
            while (true)
            {               
                delayLoop(updateInterval);
                // Check analog input on Pin A0
                 analogReading = A0.Read();
                    updateThingSpeak("field1=" + analogReading.ToString());
            }
        }

        static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("Send to twitter " + analogReading.ToString());
            //Send to twitter ...
            led.Write(true);

            var tsData = "api_key=" + thingTweetAPIkey  + "&status=" + " Overdo Index: " + analogReading.ToString();

            String request = "POST /apps/thingtweet/1/statuses/update HTTP/1.1\n";
            request += "Host: api.thingspeak.com\n";
            request += "Connection: close\n";
            
            request += "Content-Type: application/x-www-form-urlencoded\n";
            request += "Content-Length: " + tsData.Length + "\n\n";

            request += tsData;

            try
            {
                Debug.Print("Analogue data: " + analogReading.ToString());
                String tsReply = sendPOST(tsIP, tsPort, request);
                Debug.Print(tsReply);
                Debug.Print("...disconnected.\n");
                led.Write(false);
            }
            catch (SocketException se)
            {
                Debug.Print("Connection Failed.\n");
                Debug.Print("Socket Error Code: " + se.ErrorCode.ToString());
                Debug.Print(se.ToString());
                Debug.Print("\n");
                led.Write(false);
            }
        }

        static void updateThingSpeak(string tsData)
        {
            Debug.Print("Connected to ThingSpeak...\n");
            led.Write(true);

            String request = "POST /update HTTP/1.1\n";
            request += "Host: api.thingspeak.com\n";
            request += "Connection: close\n";
            request += "X-THINGSPEAKAPIKEY: " + writeAPIKey + "\n";
            request += "Content-Type: application/x-www-form-urlencoded\n";
            request += "Content-Length: " + tsData.Length + "\n\n";

            request += tsData;

            try
            {
                Debug.Print("Analogue data: " + analogReading.ToString());
                String tsReply = sendPOST(tsIP, tsPort, request);
                Debug.Print(tsReply);
                Debug.Print("...disconnected.\n");
                led.Write(false);
            }
            catch (SocketException se)
            {
                Debug.Print("Connection Failed.\n");
                Debug.Print("Socket Error Code: " + se.ErrorCode.ToString());
                Debug.Print(se.ToString());
                Debug.Print("\n");
                led.Write(false);
            }
        }

        // Issues a http POST request to the specified server. (From the .NET Micro Framework SDK example)
        private static String sendPOST(String server, Int32 port, String request)
        {
            const Int32 c_microsecondsPerSecond = 1000000;

            // Create a socket connection to the specified server and port.
            using (Socket serverSocket = ConnectSocket(server, port))
            {
                // Send request to the server.
                Byte[] bytesToSend = Encoding.UTF8.GetBytes(request);
                serverSocket.Send(bytesToSend, bytesToSend.Length, 0);

                // Reusable buffer for receiving chunks of the document.
                Byte[] buffer = new Byte[1024];

                // Accumulates the received page as it is built from the buffer.
                String page = String.Empty;

                // Wait up to 30 seconds for initial data to be available.  Throws an exception if the connection is closed with no data sent.
                DateTime timeoutAt = DateTime.Now.AddSeconds(30);
                while (serverSocket.Available == 0 && DateTime.Now < timeoutAt)
                {
                    System.Threading.Thread.Sleep(100);
                }

                // Poll for data until 30-second timeout.  Returns true for data and connection closed.
                while (serverSocket.Poll(30 * c_microsecondsPerSecond, SelectMode.SelectRead))
                {
                    // If there are 0 bytes in the buffer, then the connection is closed, or we have timed out.
                    if (serverSocket.Available == 0) break;

                    // Zero all bytes in the re-usable buffer.
                    Array.Clear(buffer, 0, buffer.Length);

                    // Read a buffer-sized HTML chunk.
                    Int32 bytesRead = serverSocket.Receive(buffer);

                    // Append the chunk to the string.
                    page = page + new String(Encoding.UTF8.GetChars(buffer));
                }

                // Return the complete string.
                return page;
            }
        }

        // Creates a socket and uses the socket to connect to the server's IP address and port. (From the .NET Micro Framework SDK example)
        private static Socket ConnectSocket(String server, Int32 port)
        {
            // Get server's IP address.
            IPHostEntry hostEntry = Dns.GetHostEntry(server);

            // Create socket and connect to the server's IP address and port
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(hostEntry.AddressList[0], port));
            return socket;
        }

        static void delayLoop(int interval)
        {
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int offset = (int)(now % interval);
            int delay = interval - offset;
            Thread.Sleep(delay);
        }
    }
}