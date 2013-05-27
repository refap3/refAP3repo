using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using System.Net;
using System.IO;
using Gadgeteer.Modules.Seeed;

namespace GadgeteerApp1
{
    public partial class Program
    {
        //CONSTANTS that aren't
        //        string reqHOST = "192.168.2.157";
        string reqHOST = "spider.hv.internal";
        // reqHOST = "localhost"; //debug focc 
        string reqPORT = "80";
        bool captureOn = false;

        // measurement variables 

        int lightMEAS = 0;
        int humMEAS = 0;
        int tempMEAS = 0;

        int secsForAutoMeasurement = 10 * 60;  //some minutes expressed as seconds 




        void ProgramStarted()
        {
            multicolorLed.GreenBlueSwapped = true;
            multicolorLed.BlinkRepeatedly(GT.Color.Blue);

            InitDisplay();
            //camera.CurrentPictureResolution = Camera.PictureResolution.Resolution160x120; 
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);

            //SET up sensors ...
            temperatureHumidity.MeasurementComplete += new TemperatureHumidity.MeasurementCompleteEventHandler(temperatureHumidity_MeasurementComplete);
            temperatureHumidity.RequestMeasurement(); 
            

            // NW and WEBSERVER setup ...
            //NW up event never fires hope we are networked already ...
            WebServer.StartLocalServer(ethernet_J11D.NetworkSettings.IPAddress, 80);
            var getVersion = WebServer.SetupWebEvent("version");
            getVersion.WebEventReceived += new WebEvent.ReceivedWebEventHandler(getVersion_WebEventReceived);
            var toggleCapture = WebServer.SetupWebEvent("toggle");
            toggleCapture.WebEventReceived += new WebEvent.ReceivedWebEventHandler(toggleCapture_WebEventReceived);
            // start the timer for automatic measurement 
            //
   
            var timer = new GT.Timer(secsForAutoMeasurement*1000);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start(); 



            multicolorLed.BlinkRepeatedly(GT.Color.Green);

        }

        void temperatureHumidity_MeasurementComplete(TemperatureHumidity sender, double temperature, double relativeHumidity)
        {

            tempMEAS = (int)temperature;
            humMEAS = (int)relativeHumidity;

            temperatureHumidity.RequestMeasurement(); 
            
        }

        void timer_Tick(GT.Timer timer)
        {
            if (captureOn)
            {
                doMeasurement(); 
            }
        }

        void toggleCapture_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            captureOn = !(captureOn);

            string content = "<html><body><h1>" + getCaptureMode() + "</h1></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");

            if (captureOn) multicolorLed.BlinkRepeatedly(GT.Color.Red);    
            else           multicolorLed.BlinkRepeatedly(GT.Color.Green); 
            

        }

        private string getCaptureMode()
        {
            return (captureOn ? "Capture Active every "+ (secsForAutoMeasurement/60).ToString() + "mins" : "Capture NOT Active");
        }

        void getVersion_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            string content = "<html><body><h1>Version 1.0!</h1>"+getCaptureMode() + "</body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes, "text/html");

        }



        private void DisplayText(int position1to3, string displText)
        {
            uint y = 160;
            switch (position1to3)
            {
                case 1: y += 0;
                    break;
                case 2: y += 20;
                    break;
                case 3: y += 40;
                    break;
                case 4: y += 60;
                    break;
                default:
                    break;
            }
            display_TE35.SimpleGraphics.DisplayText(displText, Resources.GetFont(Resources.FontResources.NinaB), GT.Color.Red, 10, y);
        }

        void camera_PictureCaptured(Camera sender, GT.Picture picture)
        {
            InitDisplay();
            display_TE35.SimpleGraphics.DisplayImage(picture, 0, 0);



            string reaURI = "http://" + reqHOST + ":" + reqPORT + "/gadg/Service1/transfer/" + lightMEAS.ToString() + "/" + humMEAS.ToString() + "/" + tempMEAS.ToString();

            POSTContent postCont = POSTContent.CreateBinaryBasedContent(picture.PictureData);
            var req = HttpHelper.CreateHttpPostRequest(reaURI, postCont, "image/bmp");

            req.ResponseReceived += new HttpRequest.ResponseHandler(req_ResponseReceived);
            req.SendRequest();

            DisplayText(2, "Req sent - PicSize: " + picture.PictureData.Length.ToString());
            DisplayText(3, "MEAS li:" + lightMEAS.ToString() + " hu:" + humMEAS.ToString() + " te:" + tempMEAS.ToString() );
        }

        private void InitDisplay()
        {
            display_TE35.SimpleGraphics.Clear();
            DisplayText(1, "IP: " + ethernet_J11D.NetworkSettings.IPAddress);
        }



        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            doMeasurement();
        }

        private void doMeasurement()
        {
            if (camera.CameraReady)
            {
                camera.TakePicture();
            }

            lightMEAS = (int)lightSensor.ReadLightSensorPercentage();
        }

        void req_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            DisplayText(4, "Response received(200 is OK): " + response.StatusCode);
        }
    }
}
