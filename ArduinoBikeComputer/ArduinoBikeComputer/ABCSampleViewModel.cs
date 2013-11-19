
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

        }

        public ABCSampleViewModel(IABCSampleSource source):this()
        {
            _source = source;
            _source.SampleArrived += new Action<ABCSample>(_sourceSampleArrived);
        }

        void _sourceSampleArrived(ABCSample sample)
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.WheelRPM = sample.WheelRPM;
                this.PedalRPM = sample.PedalRPM;
                this.WheelTemperature = sample.WheelTemperature;
                this.Power = sample.Power;
                this.Time = sample.Time;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                {
                    this.WheelRPM = sample.WheelRPM;
                    this.PedalRPM = sample.PedalRPM;
                    this.WheelTemperature = sample.WheelTemperature;
                    this.Power = sample.Power;
                    this.Time = sample.Time;
                });
            }
        }



        #region Dependency Properties
        
        public double WheelRPM
        {
            get{ return (double) GetValue(WheelRPMProperty);}
            set{ SetValue(WheelRPMProperty, value);}
        }

        // Using a DependencyProperty as the backing store for PedalRPM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WheelRPMProperty =
            DependencyProperty.Register("WheelRPM", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));



        public double PedalRPM
        {
            get { return (double)GetValue(PedalRPMProperty); }
            set { SetValue(PedalRPMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PedalRPM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PedalRPMProperty =
            DependencyProperty.Register("PedalRPM", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));



        public double WheelTemperature
        {
            get { return (double)GetValue(WheelTemperatureProperty); }
            set { SetValue(WheelTemperatureProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WheelTemperature.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WheelTemperatureProperty =
            DependencyProperty.Register("WheelTemperature", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));



        public double EnvironmentTemp
        {
            get { return (double)GetValue(EnvironmentTempProperty); }
            set { SetValue(EnvironmentTempProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnvironmentTemp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnvironmentTempProperty =
            DependencyProperty.Register("EnvironmentTemp", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));

        

        public double Power
        {
            get { return (double)GetValue(PowerProperty); }
            set { SetValue(PowerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Power.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PowerProperty =
            DependencyProperty.Register("Power", typeof(double), typeof(ABCSampleViewModel), new UIPropertyMetadata(0.0d));




        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(DateTime), typeof(ABCSampleViewModel), new UIPropertyMetadata(DateTime.Now));

        

        

        

        

        #endregion



    }
}
