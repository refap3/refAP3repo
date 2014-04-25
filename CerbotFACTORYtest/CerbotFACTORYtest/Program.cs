using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Modules.GHIElectronics;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace CerbotFACTORYtest
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.  
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing
            their name followed by a period, e.g.  button.  or  camera.
           
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
           
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            new Thread(DoEdgeDetect).Start();
            new Thread(RobotEyeSlide).Start();

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void DoEdgeDetect()
        {
            const int RUN_SPEED = 60;
            const int REVERSE_SPEED = -60;
            const double SENSOR_THRESHOLD = 10;

            while (true)
            {
                Thread.Sleep(1);

                bool leftOverEdge = cerbotController.GetReflectiveReading(CerbotController.ReflectiveSensors.Left) > SENSOR_THRESHOLD;
                bool rightOverEdge = cerbotController.GetReflectiveReading(CerbotController.ReflectiveSensors.Right) > SENSOR_THRESHOLD;

                if (!rightOverEdge && !leftOverEdge)
                {
                    cerbotController.SetMotorSpeed(RUN_SPEED, RUN_SPEED);
                    Thread.Sleep(1000);
                    cerbotController.SetMotorSpeed(REVERSE_SPEED, REVERSE_SPEED);
                    Thread.Sleep(750);
                    cerbotController.SetMotorSpeed(0, 0);

                }
            }
        }

        void RobotEyeSlide()
        {
            ushort Eyes = 0;
            int direction = 1;
            int ledIndex = 0;
            while (true)
            {
                ledIndex += direction;
                Eyes = (ushort)(0x1 << ledIndex);
                cerbotController.SetLEDBitmask(Eyes);
                Thread.Sleep(30);

                if (ledIndex <= 0 || ledIndex >= 15)
                {
                    direction *= -1;
                    cerbotController.StartBuzzer(1000);
                    Thread.Sleep(20);
                    cerbotController.StopBuzzer();
                }
            }
        }

    }
}