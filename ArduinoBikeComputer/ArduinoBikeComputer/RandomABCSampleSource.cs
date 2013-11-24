using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace ArduinoBikeComputer
{
    class RandomABCSampleSource : IABCSampleSource
    {

        DispatcherTimer timer;
        private const int TIMER_INTERVAL = 100;

        
        private readonly Object _lock = new Object();
        private Random rnd = new Random();
        private ABCSample sample = new ABCSample();

        public RandomABCSampleSource()
        {
            timer = new DispatcherTimer(DispatcherPriority.DataBind);
            timer.Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL);

            sample.PedalRPM = rnd.NextDouble() * 120.0d;
            
            sample.WheelTempC = (rnd.NextDouble() * 60.0d) + 20.0d;
            sample.EnviroTempC = 20.0;
            sample.Time = DateTime.Now;


            timer.Tick += delegate
                {
                    lock (_lock)
                    {
                        ABCSample nextSample = new ABCSample();
                        nextSample.PedalRPM = ((rnd.NextDouble() - 0.5d) * 5.0d) + sample.PedalRPM;
                        if (nextSample.PedalRPM < 0.0d)
                        {
                            nextSample.PedalRPM = 0.0d;
                        }
                        nextSample.WheelTempC = (rnd.NextDouble() - 0.5d) + sample.WheelTempC;
                        if (nextSample.WheelTempC < 0.0d)
                        {
                            nextSample.WheelTempC = 0.0d;
                        }
                        sample = nextSample; 
                        SampleArrived(sample);
 
                    }
                };
            timer.Start();
        }

        public event Action<ABCSample> SampleArrived;

    }
}
