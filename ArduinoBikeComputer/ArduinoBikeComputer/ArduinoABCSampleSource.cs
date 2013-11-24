using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandMessenger;
using System.IO;

namespace ArduinoBikeComputer
{
    class ArduinoABCSampleSource :IABCSampleSource, IDisposable
    {
        // Queue Size
        const int N = 15;

        //Environmental temp...
        Double Tmed = Double.MaxValue;
        Queue<ABCSample> envTempQueue = new Queue<ABCSample>(N);

        //Queues For determining Power.
        Queue<ABCSample> powerQueueA = new Queue<ABCSample>(N);
        Queue<ABCSample> powerQueueB = new Queue<ABCSample>(N);
        

        //ABCSample sample = new ABCSample();
        private readonly Object _lock = new Object();
        // This is only to generate the Cadence value for now.
        private Random rnd = new Random();

        SerialPortManager portManager;

        //move this to its own class eventually
        TextWriter file;
        string filename;

        public ArduinoABCSampleSource() :this("COM4", 9600)
        {
            
        }

        public ArduinoABCSampleSource(string portName, int baudRate)
        {
            filename = string.Format("C:\\Users\\rjw\\Documents\\ABC-logs\\Log-{0:yyyy-MM-dd-HH-mm-ss}.txt", System.DateTime.Now);
            
            portManager = new SerialPortManager ( '\n', '/' ) { CurrentSerialSettings = { PortName = portName, BaudRate = baudRate } };
            portManager.NewLineReceived += new EventHandler(portManager_NewLineReceived);
            portManager.StartListening();
        }

        ~ArduinoABCSampleSource()
        {
            Dispose(false);
        }

        public event Action<ABCSample> SampleArrived;

        //public event EventHandler SampleArrived;
        
        
        void portManager_NewLineReceived(object sender, EventArgs e)
        {
            SerialPortManager spm = (SerialPortManager)sender;
            if (spm != null)
            {
                string line = spm.ReadLine();
                while (line != null)
                {

                    // Consider moving this to the ABCSample after receiving the line!
                    ABCSample nextSample = new ABCSample(line);

                    // If we aren't getting environmental data from the arduino, we are going to use the lowest found!
                    if (nextSample.EnviroTempC == 0.0d)
                    {
                        nextSample.EnviroTempC = FindEnvironmentalTemp(nextSample);
                    }
                        
                    // If we aren't calculating power on the arduino, we'll do it here.
                    if (nextSample.Power == 0.0d)
                    {
                        nextSample = CalculatePower(nextSample);
                    }

                    // notify any subscribers to this of the this sample....
                    SampleArrived(nextSample);



                    // Write the sample out and get the next...
                    using (file = new StreamWriter(filename, true))
                    {
                        file.WriteLine(nextSample);
                    }
                    line = spm.ReadLine();
                }
            }

        }

        private Double FindEnvironmentalTemp(ABCSample sample)
        {
            // First we take the 1st N samples, then only take samples that lower the average...
            // it shouldn't be many when riding, but it is probably more accurate
            if ((sample.WheelTempC < Tmed) || envTempQueue.Count < N)
            {
                while (envTempQueue.Count >= N)
                {
                    envTempQueue.Dequeue();
                }
                envTempQueue.Enqueue(sample);
                Tmed = envTempQueue.Average(Sample => Sample.WheelTempC);
            }
            return Tmed;
        }

        private ABCSample CalculatePower(ABCSample sample)
        {
            while(powerQueueA.Count >= N)
            {
                // get rid of the old data
                powerQueueA.Dequeue();
            }

            while (powerQueueB.Count >= N)
            {
                powerQueueA.Enqueue(powerQueueB.Dequeue());
            }

            //From Experimental data, it appears that we lose ~.2 deg C when the wheel is moving.
            // I'm guessing that this is due to the movign air causing the reading to be different, not actually different surface temp.
            // so lets' help that:
            if (sample.PedalRPM > 10.0d)
            {
                sample.WheelTempC += 0.2d;
            }
            // making sure that the Power Queues get this value!

            powerQueueB.Enqueue(sample);

            //get average Temp and Time for each queue, once we've got data into both queues, means no power until N+1 samples.
            if (powerQueueA.Count > 0)
            {
                Double avgTempA = powerQueueA.Average(Sample => Sample.WheelTempC);
                Double avgTimeA = powerQueueA.Average(Sample => Sample.TimeL);
                Double avgTempB = powerQueueB.Average(Sample => Sample.WheelTempC);
                Double avgTimeB = powerQueueB.Average(Sample => Sample.TimeL);



                Double delt = avgTimeB - avgTimeA;
                Double delT = avgTempB - avgTempA;

                // Energy from the differences between the Averages of the 2 sets
                // using specific heat of Steel = 0.49 kJ/KgC
                // (40 lbs / 2.2 lbs/kg) * 0.49 kJ / kgC = 8.90909
                // Q = m * c * delT
                Double QI = 8.90909d * delT;

                // Energy that would have been lost due to Cooling towards the air temparture Tm
                // T (expected loss) = Tm + ( T0 - Tm) * e^(kt)  k ~= -2.29 * 10 ^-7 using experimentation. t0 = avgTempA, t1 = avgTempB
                Double Tel = Tmed + (avgTempA - Tmed) * (Math.Exp(-0.000000229d * delt));
                Double Qel = 8.90909d * (avgTempA - Tel);

                // Power = Qtot in kJ / del t in ms * 1000 ms/s * 1000 J / kJ => J/s => W
                // but lets not give credit for Power if the cadence == 0...

                if (sample.PedalRPM > 0)
                {
                    sample.Power = (QI + Qel) / delt * 1000000.0d;

                    // Now lets actually set this to the average including this one...
                    Double avgPwrB = powerQueueB.Average(Sample => Sample.Power);
                    // This stuff is reliant on that we don't start calc'ing power until we have at least one element in QueueA.
                    sample.Power = (avgPwrB * (N - 1) + sample.Power) / N;
                }
                powerQueueB.ElementAt(N - 1).Power = sample.Power;
            }
            return sample;
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
