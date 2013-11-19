using System;

namespace ArduinoBikeComputer
{


    public class ABCSample
    {
        #region Fields

        public double WheelRPM { get; set; }
        public double PedalRPM { get { return WheelRPM * 0.3198d; } }
        public double WheelTemperature { get; set; }
        
        public double Power { get { return WheelTemperature * 2.0f; } }

        public double EnvironmentTemp { get; set; }
        public DateTime Time { get; set; }

        #endregion

        public ABCSample()
        {
            Time = DateTime.Now;    // This might be coming from the Source instead?
        }



    }


}
