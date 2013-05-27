using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Gsiot.Server;
using System.Text;


// sample for webserver functionality via yaler.net relay ...
//need a reference to the GSIOT server dll (in the projects root directory)


public class HelloWeb
{
    private static OutputPort ledPort = new OutputPort(Pins.ONBOARD_LED, false);
    private static OutputPort remoteLight = new OutputPort(Pins.GPIO_PIN_D11, false);
    static bool onboardLedBlink = false;
    public static void Main()
    {
        var thdLed = new Thread(HandleLed);
        thdLed.Start();

        var digitalSensor = new DigitalSensor { InputPin = Pins.GPIO_PIN_D12 };
        var analogSensor = new AnalogSensor { InputPin = Pins.GPIO_PIN_A1, MinValue = 0.0, MaxValue = 3.3 };
        var lowPort = new OutputPort(Pins.GPIO_PIN_A0, false);
        var highPort = new OutputPort(Pins.GPIO_PIN_A2, true);

        var ledActuator = new DigitalActuator { OutputPin = Pins.GPIO_PIN_D13 };
        //need to create HTTP PUTs!

        var webServer = new HttpServer
        {
            RelayDomain = "gsiot-bcjp-yj88",
            RelaySecretKey = "HDMvyM11hAu6H6cxIaT50dL9ALWc81MYB8H/UFhV",
            RequestRouting =
            {
                { "GET /hello*", HandleHello }, //This accepts a lot of URLs
                { "GET /on", HandleOn }, 
                { "GET /off", HandleOff },
                { "POST /on", HandlePostOn },
                { "POST /off", HandlePostOff },
                { "GET /d12", new MeasuredVariable{ FromSensor=digitalSensor.HandleGet}.HandleRequest},
                { "GET /a1", new MeasuredVariable{ FromSensor=analogSensor.HandleGet}.HandleRequest},
                { "PUT /d13", new ManipulatedVariable{ FromHttpRequest=CSharpRepresentation.TryDeserializeBool,ToActuator=ledActuator.HandlePut}.HandleRequest},
            }
        };
        webServer.Run();

    }

    static void HandleHello(RequestHandlerContext context)
    {
        onboardLedBlink = !onboardLedBlink;

        var a1Uri = "/" + context.RequestUri.Split('/')[1] + "/a1";
        var d12Uri = "/" + context.RequestUri.Split('/')[1] + "/d12";



        string s = "<h1>Hello via Relay<h2>Led Blink is " + onboardLedBlink.ToString() + "<p> RequestUri is: " + context.RequestUri + "<p> Try <a href='" + a1Uri + "'>Analog Input A1</a> and <a href='" + d12Uri + "'>Digital Input D12</a> for sensors as well<p> PUT to /d13 for led Actuator on Pin 13!";

        s = s + "<p> use a PUT generator like <a href='http://www.hurl.it/hurls/90adc685db37c71ccb9576b0d115fbc8809faa1c/aea0240d19dc34535db35a9e64ed00b195d3bb53'>this to turn off,</a>";
        s = s + "<p> or like <a href='http://www.hurl.it/hurls/1bf8d43ca68c6a17ebdf77409c437031fe21d8e5/76fe79f73b808d69dddbb8caffd070ee9e90137b'>this to turn on.</a>";
        s = s + "<p>In order to use this fuctionality in a trigger to D11 from Xively you must create POST requests for <a href='http://www.hurl.it/hurls/3703de296f48844bef801e59e3486229e805f462/f5b7a99f9e6ec038f0196f4945f539ebbbdcc9cb#'>on</a> and <a href='http://www.hurl.it/hurls/8a8463b4d5900d51fe8a34cbf4d51d20ebd16c50/3211ff2d1b035f914f9af5431f5e133c5b1d5001'>off</a>.";

        context.SetResponse(s, "text/html");
    }
    static void HandleOn(RequestHandlerContext context)
    {
        onboardLedBlink = true;
        string s = "<h1>HANDLING LED<h2>Led Blink is ON now";
        context.SetResponse(s, "text/html");
    }
    static void HandleOff(RequestHandlerContext context)
    {
        onboardLedBlink = false;
        string s = "<h1>HANDLING LED<h2>Led Blink is OFF now";
        context.SetResponse(s, "text/html");
    }

    static void HandlePostOn(RequestHandlerContext context)
    {
        context.ResponseStatusCode = 200;
        remoteLight.Write(true);
    }
    static void HandlePostOff(RequestHandlerContext context)
    {
        context.ResponseStatusCode = 200;
        remoteLight.Write(false);
    }

    static void HandleLed()
    {

        while (true)
        {
            Thread.Sleep(500);
            ledPort.Write(onboardLedBlink);
            Thread.Sleep(500);
            ledPort.Write(false);

        }
    }









}


