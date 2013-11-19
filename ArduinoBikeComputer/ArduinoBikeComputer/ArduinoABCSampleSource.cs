using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandMessenger;

namespace ArduinoBikeComputer
{
    class ArduinoABCSampleSource :IABCSampleSource, IDisposable
    {


        ABCSample sample = new ABCSample();
        private readonly Object _lock = new Object();
        // This is only to generate the Cadence value for now.
        private Random rnd = new Random();

        SerialPortManager portManager;

        public ArduinoABCSampleSource() :this("COM3", 9600)
        {
        }

        public ArduinoABCSampleSource(string portName, int baudRate)
        {
            portManager = new SerialPortManager { CurrentSerialSettings = { PortName = portName, BaudRate = baudRate } };
            portManager.NewLineReceived += new EventHandler(portManager_NewLineReceived);
            portManager.StartListening();
        }

        ~ArduinoABCSampleSource()
        {
            Dispose(false);
        }

        public event Action<ABCSample> SampleArrived;

        void portManager_NewLineReceived(object sender, EventArgs e)
        {
            SerialPortManager spm = (SerialPortManager)sender;
            if (spm != null)
            {
                string line = spm.ReadLine();
                while (line != null)
                {


                    // Consider moving this to the ABCSample after receiving the line!
                    ABCSample nextSample = new ABCSample();

                    // Lets fix this idiocy next!
                    line = line.TrimEnd(new char[] { ';' });
                    double voltage;
                    if (Double.TryParse(line, out voltage))
                    {
                        double tempC = (voltage - 0.5) * 100;
                        double tempF = tempC * 9.0 / 5.0 + 32.0;

                        // Random generation...
                        nextSample.WheelRPM = ((rnd.NextDouble() - 0.5d) * 5.0d) + sample.WheelRPM;
                        if (nextSample.WheelRPM < 0.0d)
                        {
                            nextSample.WheelRPM = 0.0d;
                        }

                        nextSample.WheelTemperature = tempF;
                        nextSample.EnvironmentTemp = tempF;
                       
                        sample = nextSample;
                        SampleArrived(sample);

                    }
                }
            }

        }


        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose( bool  disposing)
        {
            if (disposing)
            {
                if (portManager != null)
                {
                    portManager.Dispose();
                    portManager = null;
                }
            }
        }
        #endregion

    }
}
