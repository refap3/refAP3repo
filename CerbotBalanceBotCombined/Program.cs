//this is the combined version ... see button event handler for details ...


using System;
using System.Threading;
using Microsoft.SPOT;

using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Modules.Seeed;

namespace FEZBalanceBot
{
    public partial class Program
    {
        const double RagToDeg = 57.295779513082320876798154814105;
        //        const int MotorSpeedFactor = 15;
        const int MotorSpeedFactor = 15;

        //OWN consts 
        //        const int SPEEDCOMPENSATION = -1;

        // PID factors
        const double Kp = 1;
        const double Ki = 0.5;
        const double Kd = 0.2;

        // Complementart Filter 
        //        const double GyroFactor = 0.98;
        const double GyroFactor = 0.98;
        const double AccelFactor = 1 - GyroFactor;

        // PID state variables
        double _prevError;
        double _errorSum;

        // Target angle to maintan balance
        // Tune this to compensate for the balance offset of the FEZ Cerbot
        //        const double TargetAngle = 270.81;
        const double TargetAngle = 270.81;

        DateTime _gyroLastReadTime;
        double _angle = 0;
        bool _balancing = false;

        int _speed = 0;
        int _lastSpeed = 0;

        bool running = false;

        int buttonHitCounter = 0;

        //EDge detect vars ...
        int RUN_SPEED = 60;
        const int REVERSE_SPEED = -50;
        const int REVERSE_TIME = 600;
        const int TURN_TIME = 150;
        const double SENSOR_THRESHOLD = 10;
        const int EXTRA_TURN_THRESHOLD = 5000;



        void ProgramStarted()
        {
            //IR rec 

            irReceiver.IREvent += new IR_Receiver.IREventDelegate(irReceiver_IREvent);


            button.ButtonPressed += button_ButtonPressed;
            new Thread(DoEdgeDetect).Start();
            DoBalanceBot();
        }

        void irReceiver_IREvent(object sender, IR_Receiver.IREventArgs e)
        {
            Debug.Print(e.Button.ToString());
            switch (e.Button)
            {
                case 32: //more speed on edge DoEdgeDetect
                    RUN_SPEED += 5;
                    break;

                case 33: //less speed on edge DoEdgeDetect
                    RUN_SPEED -= 5;
                    break;
                case 12: //power
                    buttonHitCounter += 1;
                    if (buttonHitCounter > 3) buttonHitCounter = 0;
                    switch (buttonHitCounter)
                    {
                        case 0:
                            running = false;
                            StopBalancing();
                            break;
                        case 1:
                            _balancing = true;
                            StartBalancing();
                            break;
                        case 2:
                            StopBalancing();
                            break;
                        case 3:
                            running = true;
                            break;

                    }
                    break;
            }

        }


        void DoBalanceBot()
        {
            // Calibrate the sensors on start-up
            Notify(1);
            Thread.Sleep(1000);
            Notify(1);
            Debug.Print("Calibrating");

            accelerometer.MeasurementRange = GTM.Seeed.Accelerometer.Range.TwoG;
            gyro.LowPassFilter = GTM.Seeed.Gyro.Bandwidth._256Hz;

            // Prep the accelerometer
            accelerometer.RequestMeasurement();

            gyro.Calibrate();
            accelerometer.Calibrate();

            Debug.Print("Calibration complete");
            Thread.Sleep(1000); Notify(2);


        }

        void button_ButtonPressed(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
        {



        }

        private void StopBalancing()
        {
            // If we were already balancing and the button was pressed again
            // we shut everything down. This is like a safety switch.
            controller.SetMotorSpeed(0, 0);
            gyro.StopContinuousMeasurements();
            _balancing = false;
        }

        private void StartBalancing()
        {
            // Start a thread to handle the balancing process
            new Thread(() =>
              {
                  // Setup the gyro
                  gyro.ContinuousMeasurementInterval = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 25);
                  gyro.MeasurementComplete += gyro_MeasurementComplete;

                  // Get the initial angle of the cerbot from the accelerometer as a starting point
                  // you should hold the cerbot as still as possible at this stage, but it is not overly
                  // sensitive
                  var acceleration = accelerometer.RequestMeasurement();
                  var accelAngle = (System.Math.Atan2(acceleration.Y, acceleration.Z) + System.Math.PI) * RagToDeg;
                  _angle = accelAngle;

                  // Startup the gyro read timer
                  _gyroLastReadTime = DateTime.Now;
                  gyro.StartContinuousMeasurements();

                  // Spin in a loop while we are trying to balance and adjust the motor speed
                  // if there is a new speed reading from the calculations.
                  while (_balancing)
                  {
                      if (_speed != _lastSpeed)
                      {
                          int _s = _speed;//+ SPEEDCOMPENSATION; 
                          controller.SetMotorSpeed(_s, _s);
                          _lastSpeed = _speed;
                      }
                      Thread.Sleep(0);
                  }
              }).Start();
        }

        void gyro_MeasurementComplete(GTM.Seeed.Gyro sender, GTM.Seeed.Gyro.SensorData sensorData)
        {
            // Grab the gyro reading and run our balancing calculations

            // Calculate the time delta since the last reading
            DateTime now = DateTime.Now;
            double dt = (now.Ticks - _gyroLastReadTime.Ticks) / (double)TimeSpan.TicksPerSecond;
            _gyroLastReadTime = now;

            // Get the current accelerometer readings
            var acceleration = accelerometer.RequestMeasurement();
            var accelAngle = (System.Math.Atan2(acceleration.Y, acceleration.Z) + System.Math.PI) * RagToDeg;

            // Use a complementary filter to combine the data from the gyro and the accelerometer to
            // compensate for the gyro drift. This gives us a reasonably accurate reading for the current angle of the
            // FEX Cerbot
            _angle = GyroFactor * (_angle + sensorData.Y * dt) + AccelFactor * accelAngle;

            // Calculate the PID values. 
            //  P - error
            //  I - errorSum
            //  D - d
            double error = (TargetAngle - _angle);
            _errorSum += error;
            var d = (error - _prevError);

            // Remember this error value for the next iteration
            _prevError = error;

            // Calculate the PID output
            double output = Kp * error + Ki * _errorSum + Kd * d;

            // Calculate the required motor speed based on the PID output
            // This value is picked up in our control thread which will actually set the motor speed
            _speed = (int)(MotorSpeedFactor * output + 0.5);

            // Ensure the motor speed does not exceed the allowed values
            if (_speed > 100) _speed = 100;
            if (_speed < -100) _speed = -100;
        }

        /// <summary>
        /// Generate notification tones on the piezo device
        /// </summary>
        /// <param name="count">The number of tones to generate</param>
        void Notify(int count)
        {
            const int duration = 100;

            for (int i = 0; i < count; i++)
            {
                controller.StartBuzzer(440, duration);
                Thread.Sleep(duration);
            }
        }
        void DoEdgeDetect()
        {
            bool correctingFromEdge = false;
            bool nextIsLeft = false;
            DateTime lastCorrection = DateTime.Now;

            // If you have a Gadgeteer button to attach, comment out or remove the
            // following line, so you robot will sit until you press the button.
            // Thread.Sleep(5 * 1000); running = true;
            while (true)
            {
                while (running)
                {
                    Thread.Sleep(1);

                    bool leftOverEdge = controller.GetReflectiveReading(CerbotController.ReflectiveSensors.Left) < SENSOR_THRESHOLD;
                    bool rightOverEdge = controller.GetReflectiveReading(CerbotController.ReflectiveSensors.Right) < SENSOR_THRESHOLD;

                    controller.SetLEDBitmask((ushort)(((leftOverEdge ? 1 : 0) << 0) + ((rightOverEdge ? 1 : 0) << 15)));

                    if (!rightOverEdge && !leftOverEdge)
                    {
                        controller.SetMotorSpeed(RUN_SPEED, RUN_SPEED);

                        correctingFromEdge = false;
                    }
                    else if (!correctingFromEdge)
                    {
                        controller.SetMotorSpeed(REVERSE_SPEED, REVERSE_SPEED);

                        Thread.Sleep(REVERSE_TIME);

                        bool isLeft = leftOverEdge;
                        if (leftOverEdge && rightOverEdge)
                        {
                            isLeft = nextIsLeft;
                            nextIsLeft = !nextIsLeft;
                        }

                        controller.SetMotorSpeed(isLeft ? RUN_SPEED : REVERSE_SPEED, isLeft ? REVERSE_SPEED : RUN_SPEED);

                        TimeSpan interval = DateTime.Now - lastCorrection;

                        Thread.Sleep(TURN_TIME + (interval.Milliseconds + interval.Seconds * 1000 < EXTRA_TURN_THRESHOLD ? TURN_TIME : 0));

                        lastCorrection = DateTime.Now;
                        correctingFromEdge = true;
                    }
                }

                controller.SetMotorSpeed(0, 0);
                controller.SetLEDBitmask(0x0);
                Thread.Sleep(500);
            }
        }


    }

}
