
using System;
using System.Windows;
using System.Windows.Threading;

namespace ArduinoBikeComputer
{
    public class ABCSampleViewModel : DependencyObject
    {
        IABCSampleSource _source;

        public ABCSampleViewModel()
        {
            Console.WriteLine("WTF: How am I getting called");
        }

        public ABCSampleViewModel(IABCSampleSource source):this()
        {
            _source = source;
            _source.SampleArrived += new Action<ABCSample>(_source_SampleArrived);
            
        }

        public void _source_SampleArrived(ABCSample sample)
        {
            if (sample != null)
            {
                if (this.Dispatcher.CheckAccess())
                {
                    this.PedalRPM = sample.PedalRPM;
                    this.WheelTempC = sample.WheelTempC;
                    this.WheelTempF = sample.WheelTempF;
                    this.Power = sample.Power;
                    this.EnviroTempC = sample.EnviroTempC;
                    this.EnviroTempF = sample.EnviroTempF;
                    this.Time = sample.Time;
                    this.TimeL = sample.TimeL;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        this.PedalRPM = sample.PedalRPM;
                        this.WheelTempC = sample.WheelTempC;
                        this.WheelTempF = sample.WheelTempF;
                        this.Power = sample.Power;
                        this.EnviroTempC = sample.EnviroTempC;
                        this.EnviroTempF = sample.EnviroTempF;
                        this.Time = sample.Time;
                        this.TimeL = sample.TimeL;
                    });
                }
            }

        }



        #region Dependency Properties
        public double PedalRPM
        {
            get { return (double)GetValue(PedalRPMProperty); }
            set { SetValue(PedalRPMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PedalRPM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PedalRPMProperty =
            DependencyProperty.Register("PedalRPM", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));


        public double WheelTempC
        {
            get{ return (double) GetValue(WheelTempCProperty);}
            set{ SetValue(WheelTempCProperty, value);}
        }

        // Using a DependencyProperty as the backing store for PedalRPM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WheelTempCProperty =
            DependencyProperty.Register("WheelTempC", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));

        public double WheelTempF
        {
            get { return (double)GetValue(WheelTempFProperty); }
            set { SetValue(WheelTempFProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WheelTemperature.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WheelTempFProperty =
            DependencyProperty.Register("WheelTempF", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));


        public double Power
        {
            get { return (double)GetValue(PowerProperty); }
            set { SetValue(PowerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Power.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PowerProperty =
            DependencyProperty.Register("Power", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));

        
        public double EnviroTempC
        {
            get { return (double)GetValue(EnviroTempCProperty); }
            set { SetValue(EnviroTempCProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnvironmentTemp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnviroTempCProperty =
            DependencyProperty.Register("EnviroTempC", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));



        public double EnviroTempF
        {
            get { return (double)GetValue(EnviroTempFProperty); }
            set { SetValue(EnviroTempFProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnvironmentTemp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnviroTempFProperty =
            DependencyProperty.Register("EnviroTempF", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));


        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(DateTime), typeof(ABCSampleViewModel), new UIPropertyMetadata(DateTime.Now));


        public long TimeL
        {
            get { return (long)GetValue(TimeLProperty); }
            set { SetValue(TimeLProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeLProperty =
            DependencyProperty.Register("TimeL", typeof(long), typeof(ABCSampleViewModel), new UIPropertyMetadata(0L));

        #endregion

    }

}
