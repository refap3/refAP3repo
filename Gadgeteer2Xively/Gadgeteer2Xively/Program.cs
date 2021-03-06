﻿using System;
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
using Gadgeteer.Modules.Seeed;
using System.Text;
using System.Net;

namespace Gadgeteer2Xively
{
    public partial class Program
    {
        static double tempMeas;
        static double humMeas;
        static double lightMeas;
        static double compassAngleMeas;
        static double barometerMeas;
        static double potentiometerMeas; 
        string xyvStatus;
        static double countDown = 0;
        static GT.Timer timer;


        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");
            multicolorLed.BlinkRepeatedly(GT.Color.Blue);
            ethernet_J11D.NetworkUp += new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_J11D_NetworkUp);
            // removed NW down handler 
            //ethernet_J11D.NetworkDown += new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_J11D_NetworkDown);
            temperatureHumidity.MeasurementComplete += new TemperatureHumidity.MeasurementCompleteEventHandler(temperatureHumidity_MeasurementComplete);
            barometer.MeasurementComplete += new Barometer.MeasurementCompleteEventHandler(barometer_MeasurementComplete);
            barometer.ContinuousMeasurementInterval = new TimeSpan(0, 0, 0, 0, 500);
            barometer.StartContinuousMeasurements();
            compass.MeasurementComplete += new Compass.MeasurementCompleteEventHandler(compass_MeasurementComplete);
            compass.ContinuousMeasurementInterval = new TimeSpan(0, 0, 0, 0, 500);
            compass.StartContinuousMeasurements();

            ethernet_J11D.UseDHCP();
            timer = new GT.Timer(1000);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick); // timer will be started after NW up 
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            
        }

        void compass_MeasurementComplete(Compass sender, Compass.SensorData sensorData)
        {
            compassAngleMeas = sensorData.Angle; 
        }

        void barometer_MeasurementComplete(Barometer sender, Barometer.SensorData sensorData)
        {
            barometerMeas = sensorData.Pressure; 
        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            ExecuteMeasurementCycle();

        }

        private void ExecuteMeasurementCycle()
        {
            //run in seperate Thread since this sometimes hangs in request.getstream ...
            //

                var t = new Thread(ExecuteMeasurementCycleInSeperateThread);
                t.Start();  
        }

        private void ExecuteMeasurementCycleInSeperateThread()
        {
            //run upload to Xivley
            Debug.Print("About to submit to Xively ...");
            xyvStatus = SubmitToXively(tempMeas, humMeas, lightMeas, compassAngleMeas , barometerMeas, potentiometerMeas );
            Debug.Print("Xivley returned; " + xyvStatus);
            if (xyvStatus == "200") multicolorLed.BlinkOnce(GT.Color.Orange, new TimeSpan(0, 0, 5), GT.Color.Green);
            else multicolorLed.BlinkOnce(GT.Color.Red, new TimeSpan(0, 0, 1), GT.Color.Green);
        }

        void temperatureHumidity_MeasurementComplete(TemperatureHumidity sender, double temperature, double relativeHumidity)
        {
            tempMeas = temperature;
            humMeas = relativeHumidity;
        }

        void timer_Tick(GT.Timer timer)
        {
            countDown--;
            if (countDown <= 0)
            {
                countDown = potentiometer.ReadPotentiometerPercentage() * 100 + 10 ;
                ExecuteMeasurementCycle();
            }
            temperatureHumidity.RequestMeasurement();
            lightMeas = lightSensor.ReadLightSensorPercentage() * 100;
            potentiometerMeas = potentiometer.ReadPotentiometerPercentage();
            UpdateDisplay();

        }

        private void UpdateDisplay()
        {
            ClearDisplay();
            DisplayText(2, "Last Xyve: " + xyvStatus);
            DisplayText(3, "T: " + tempMeas.ToString("F2") + " H: " + humMeas.ToString("F2") + " L: " + lightMeas.ToString("F2") + " C: " + compassAngleMeas.ToString("F2") + " P: " + barometerMeas.ToString("F2"));
            DisplayText(4, "Pot: " + (potentiometer.ReadPotentiometerPercentage() * 100).ToString("F2") + " Countdown: " + countDown.ToString("F2"));

        }

        //void ethernet_J11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        //{
        //    multicolorLed.TurnRed();
        //}

        void ethernet_J11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            // wait for NW to settle ...
            Thread.Sleep(2500);
            multicolorLed.TurnGreen();
            ClearDisplay();
            // start timer after NW up ...
            timer.Start();
        }

        //display stuff ...
        private void ClearDisplay()
        {
            display_TE35.SimpleGraphics.Clear();
            DisplayText(1, ethernet_J11D.NetworkSettings.IPAddress.ToString());
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

        private  string  SubmitToXively(double tempMeas, double humMeas, double lightMeas, double angleMeas, double pressMeas, double potMeas)
        {

            const string NEWLINE = "\n";

            const string APIKEY = "820f-wFPt2yWqCxSwk4t3gvP4F2SAKw4V2s3TEsycGJhVT0g";
            const string APIENDPOINT = "http://api.xively.com/v2/feeds/1934589243";

            // deliver the  measurements to xively
            string result = "Humidity, " + humMeas.ToString("F2") + NEWLINE + "  Light, " + lightMeas.ToString("F2") + NEWLINE + "  Temperature, " + tempMeas.ToString("F2") + NEWLINE + "  Compass, " + angleMeas.ToString("F2") + NEWLINE + "  Pressure, " + pressMeas.ToString("F2") + NEWLINE + "Potentiometer, " + potentiometerMeas.ToString("F2") ;

            byte[] bytes = Encoding.UTF8.GetBytes(result);
            string strRequestUri = APIENDPOINT + ".csv";
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

                        return response.StatusCode.ToString();
                    }
                }
            }
            catch (Exception xx)
            {
                return xx.ToString();
            }
        }



    }
}
