using System;

namespace ArduinoBikeComputer
{


    public class ABCSample
    {
        #region Fields

        public double PedalRPM { get; set; }
        public double WheelTempC { get; set; }
        public double WheelTempF { get { return 1.8 * WheelTempC + 32.0; }  }
        
        public double Power { get; set; }

        public double EnviroTempC { get; set; }
        public double EnviroTempF { get { return 1.8 * EnviroTempC + 32.0; } }
        public DateTime Time { get; set; }
        
        public long TimeL { get; set; }     // milliseconds since start of arduino. this might be different from the file version.... ewhich might be an offset since start of recording?
        public long LastTimeL { get; set; }
        public long FirstTimeL { get; set; }

        public long RevCount { get; set; }
        public Double DistanceKm { get { return RevCount * 0.00179767; } }  // Dist = 2*pi*r * Chain ring size / rear sprockect size -> distance tarvalled per pedal rev.
                                                                            // D = 680 mm * pi * 50 T / 19 T = 5621.8 mm
                                                                            // PedalRev to WheelRev -> 22/68.8  => 1797.67 mm
        public Double DistanceMi { get { return DistanceKm / 1.60934;  } }

        public Double SpeedKm { get { return PedalRPM > 0.0d ? 5.6128 * PedalRPM * 60.0 / 1000.0 : 0.0d; } }
        public Double SpeedMi { get { return SpeedKm / 1.60934; } }


        public TimeSpan TimeRiding { get { return new TimeSpan(0, 0, 0, 0, (int)(TimeL - FirstTimeL)); } }


        #endregion

        public ABCSample()
        {
            Time = DateTime.Now;    // This might be coming from the Source instead?
        }

        public ABCSample(string sampleline)
        {
            SetBySampleString(sampleline);
            Time = DateTime.Now;

        }

        /// <summary>
        /// Takes a sample string and sets the Samples fields based on the values found
        /// </summary>
        /// <param name="sampleline">sample line of the form "C:123.45~T:45.232~t:67875...\r\n"</param>
        public void SetBySampleString(string sampleline)
        {
            // Lets get rid of the "### Temp ### ";
            if (sampleline.StartsWith("### Temp ### "))
            {
                sampleline = sampleline.Substring(13);
            }

            sampleline = sampleline.TrimEnd('\r', '\n');
            string[] points = sampleline.Split('~');

            foreach (string datapoint in points)
            {
                switch (datapoint[0])
                {
                    case 'C':
                        PedalRPM = ParseDoubleDataPoint(datapoint, datapoint[0]);
                        break;
                    case 'W':
                        WheelTempC = ParseDoubleDataPoint(datapoint, datapoint[0]);
                        break;
                    case 't':
                        TimeL = ParseLongDataPoint(datapoint, datapoint[0]);
                        break;
                    case 'T':
                        //Time = ParseSomethingHere...
                        break;
                    case 'P':
                        Power = ParseDoubleDataPoint(datapoint, datapoint[0]);
                        break;
                    case 'E':
                        EnviroTempC = ParseDoubleDataPoint(datapoint, datapoint[0]);
                        break;
                    default:
                        break;
                }
            }
        }

        private double ParseDoubleDataPoint(string datapoint, char id)
        {
            double retVal = 0.0;
            datapoint = datapoint.TrimStart(id, ':');
            return Double.TryParse( datapoint, out retVal) ? retVal: 0.0;
        }

        private long ParseLongDataPoint(string datapoint, char id)
        {
            long retVal = 0;
            datapoint = datapoint.TrimStart(id, ':');
            return Int64.TryParse(datapoint, out retVal) ? retVal : 0;
        }

        public override string ToString()
        {
            return String.Format("T:{0:yyyy-MM-dd HH-mm-ss.fff}~t:{1}~C:{2}~W:{3}~E:{4}~P:{5}", Time, TimeL, PedalRPM, WheelTempC, EnviroTempC, Power);
        }


    }


}
