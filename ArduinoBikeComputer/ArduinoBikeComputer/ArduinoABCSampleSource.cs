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

        // Distance between Power Calcs
        const long T = 60000;


        //Environmental temp...
        Double Tmed = Double.MaxValue;
        Queue<ABCSample> envTempQueue = new Queue<ABCSample>(N);

        //Queues For determining Power.
        Queue<ABCSample> holdingQueue = new Queue<ABCSample>(4*N);
        ABCSample LastPowerSample = new ABCSample();
        Queue<ABCSample> powerQueueA = new Queue<ABCSample>(N);
        Queue<ABCSample> powerQueueB = new Queue<ABCSample>(N);

        //Queue<double> avgPowerQueue = new Queue<double>(N);

        ABCSample LastSample = new ABCSample();
        ABCSample FirstMovingSample = null;
        Double TotalEnergy = 0.0;


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



                    // Time, Speed and Distance 
                    if (FirstMovingSample == null && nextSample.PedalRPM > 0.0d)
                    {
                        FirstMovingSample = nextSample;
                    }

                    if (FirstMovingSample != null)
                    {
                        nextSample.FirstTimeL = FirstMovingSample.TimeL;
                    }
                    else
                    {
                        nextSample.FirstTimeL = nextSample.TimeL;
                    }


                    nextSample.LastTimeL = LastSample.TimeL;

                    if (nextSample.PedalRPM > 0.0)
                    {
                        nextSample.RevCount = LastSample.RevCount + 1;
                    }
                    else
                    {
                        nextSample.RevCount = LastSample.RevCount;
                    }

                    // notify any subscribers to this of the this sample....
                    SampleArrived(nextSample);

                    // Keep this guy for the T/S/D calcs
                    LastSample = nextSample;

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
            holdingQueue.Enqueue(sample);

            // lets find out how old the oldest is...
            ABCSample oldest = holdingQueue.Peek();

            // Version 3!
            // Take 20 Readings, finding mid point between min and max temp.
            // This is trying to compensate for the differences in readings across the wheel being upto 10F!
            // It'll be lower, but less jittery (?)
            if (holdingQueue.Count >= 80)
            {
                //clear any existing...
                powerQueueA.Clear();
                powerQueueB.Clear();
                int queueSize = 20;

                // Load up the First
                while (powerQueueA.Count < queueSize && holdingQueue.Count > 0)
                {
                    powerQueueA.Enqueue(holdingQueue.Dequeue());
                }

                while (holdingQueue.Count > queueSize)
                {
                    holdingQueue.Dequeue();
                }

                // Load up the Last
                while (holdingQueue.Count > 0)
                {
                    powerQueueB.Enqueue(holdingQueue.Dequeue());
                }
                
                if (powerQueueA.Count > 0)
                {

                    // Lets get the values that we'll use to make our calcaulations
                    Double maxTempA = powerQueueA.Max(Sample => Sample.WheelTempC);
                    Double minTempA = powerQueueA.Max(Sample => Sample.WheelTempC);
                    Double midTempA = (maxTempA + minTempA) / 2.0;
                    Double maxTimeA = powerQueueA.Max(Sample => Sample.TimeL);
                    Double minTimeA = powerQueueA.Max(Sample => Sample.TimeL);
                    Double midTimeA = (maxTimeA + minTimeA) / 2.0;

                    Double maxTempB = powerQueueB.Max(Sample => Sample.WheelTempC);
                    Double minTempB = powerQueueB.Max(Sample => Sample.WheelTempC);
                    Double midTempB = (maxTempB + minTempB) / 2.0;
                    Double maxTimeB = powerQueueB.Max(Sample => Sample.TimeL);
                    Double minTimeB = powerQueueB.Max(Sample => Sample.TimeL);
                    Double midTimeB = (maxTimeB + minTimeB) / 2.0;

                    Double delt = midTimeB - midTimeA;
                    Double delT = midTempB - midTempA;

                    // Energy from the differences between the Averages of the 2 sets
                    // using specific heat of Steel = 0.49 kJ/KgC
                    // (40 lbs / 2.2 lbs/kg) * 0.49 kJ / kgC = 8.90909
                    // Q = m * c * delT
                    Double QI = 8.90909d * delT;
                    // Lets not take any penalty for bad data!
                    if (QI < 0.0)
                    {
                        QI = 0.0;
                    }

                    // Energy that would have been lost due to Cooling towards the air temparture Tm
                    // T (expected loss) = Tm + ( T0 - Tm) * e^(kt)  k ~= -2.29 * 10 ^-7 using experimentation. t0 = avgTempA, t1 = avgTempB
                    Double Tel = Tmed + (midTempA - Tmed) * (Math.Exp(-0.000000229d * delt));
                    Double Qel = 8.90909d * (midTempA - Tel);
                    
                    // Lets not take any penalty for bad data!
                    if (Qel < 0.0)
                    {
                        Qel = 0.0;
                    }

                    // How about the power difference based on kinetic energy in the wheel!
                    // I ~= 3/4 MR^2 based on shape
                    // Krot = 1/2 Iw^2
                    // M  = 18 kg
                    // R = .23 m
                    // I = 3/4 * 18 * (.23)^2 = .714 kg m^2
                    // w = (Cadence * 68.8/22 * 2pi/60 )^2
                    // Experimentation with no resistance on the wheel leads to -2.8 rpms / sec lost when allowed to spin to zero.
                    
                    Double avgCadA = powerQueueA.Average(Sample => Sample.PedalRPM);
                    Double avgCadB = powerQueueB.Average(Sample => Sample.PedalRPM);
                    
                    Double w = ((avgCadB - avgCadA) * 0.327321);

                    // Not going include the neagtive difference, because any negative change in cadence will also requires some "breaking" effect
                    Double KI = w > 0.0 ? .357 * w * w : 0.0;
                    Double elw = ((2.8 * delt / 1000.0) * 0.327321);
                    Double Kel = .357 * elw * elw;
                    // These are in J, so lets get them into KJ so is works with the Qs.
                    KI /= 1000.0;
                    Kel /= 1000.0;
                    


                    // Power = Qtot in kJ / del t in ms * 1000 ms/s * 1000 J / kJ => J/s => W
                    // but lets not give credit for Power if the cadence == 0...
                    if (sample.PedalRPM > 0)
                    {
                        sample.Power = (QI + Qel + KI + Kel) / delt * 1000000.0d;
                        TotalEnergy += (QI + Qel + KI + Kel);
                        //avgPowerQueue.Enqueue(sample.Power);
                        sample.AveragePower = TotalEnergy / sample.TimeRiding.Seconds * 1000.0;
                    }

                    // Lets push the samples from Queue B, back on
                    while (powerQueueB.Count > 0)
                    {
                        holdingQueue.Enqueue(powerQueueB.Dequeue());
                    }

                    Console.WriteLine("TempA: {0:0.00} TempB: {1:0.00} del t: {2:0.00} delT: {3:0.0} QI: {4:0.00} Tel:{5:0.00} Qel:{6:0.00} KI: {7:0.00} Kel: {8:0.00}"
                                                        , midTempA, midTempB, delt, delT, QI, Tel, Qel, KI, Kel);
                                            
                }
                LastPowerSample = sample;

            }
            else
            {
                sample.Power = LastPowerSample.Power;
                sample.AveragePower = LastPowerSample.AveragePower;
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
