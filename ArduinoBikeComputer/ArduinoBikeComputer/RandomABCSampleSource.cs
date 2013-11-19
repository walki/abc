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

            sample.WheelRPM = rnd.NextDouble() * 360.0d;
            
            sample.WheelTemperature = (rnd.NextDouble() * 60.0d) + 65.0d;
            
            sample.Time = DateTime.Now;


            timer.Tick += delegate
                {
                    lock (_lock)
                    {
                        ABCSample nextSample = new ABCSample();
                        nextSample.WheelRPM = ((rnd.NextDouble() - 0.5d) * 5.0d) + sample.WheelRPM;
                        if (nextSample.WheelRPM < 0.0d)
                        {
                            nextSample.WheelRPM = 0.0d;
                        }
                        nextSample.WheelTemperature = (rnd.NextDouble() - 0.5d) + sample.WheelTemperature;
                        if (nextSample.WheelTemperature < 0.0d)
                        {
                            nextSample.WheelTemperature = 0.0d;
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
