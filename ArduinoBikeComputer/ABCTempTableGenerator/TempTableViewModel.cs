using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ABCTempTableGenerator
{
    public class TempTableViewModel :DependencyObject
    {
        public TempTableViewModel()
        {

            this.CalculateCommand = new CalculateCommand(this);
        }

      
        
        
        TempTimeList ttList = new TempTimeList();

        public TempTimeList TempTimes { get {return ttList;} }

        #region Dependency Props
        public Double TempM
        {
            get { return (Double)GetValue(TempMProperty); }
            set { SetValue(TempMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TempM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TempMProperty =
            DependencyProperty.Register("TempM", typeof(Double), typeof(TempTableViewModel), new UIPropertyMetadata(0.0d));


        public Double Temp0
        {
            get { return (Double)GetValue(Temp0Property); }
            set { SetValue(Temp0Property, value); }
        }

        // Using a DependencyProperty as the backing store for Temp0.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Temp0Property =
            DependencyProperty.Register("Temp0", typeof(Double), typeof(TempTableViewModel), new UIPropertyMetadata(0.0d));


        public Double Temp1
        {
            get { return (Double)GetValue(Temp1Property); }
            set { SetValue(Temp1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Temp1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Temp1Property =
            DependencyProperty.Register("Temp1", typeof(Double), typeof(TempTableViewModel), new UIPropertyMetadata(0.0d));


        public long time1
        {
            get { return (long)GetValue(time1Property); }
            set { SetValue(time1Property, value); }
        }

        // Using a DependencyProperty as the backing store for time1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty time1Property =
            DependencyProperty.Register("time1", typeof(long), typeof(TempTableViewModel), new UIPropertyMetadata(0L));
        #endregion




        public ICommand CalculateCommand { get; set; }

        internal bool CanCalculate()
        {
            bool retVal = false;
            if (TempM != Temp0 && Temp0 != Temp1 && Temp1 != TempM && time1 != 0)
            {
                retVal = true;
            }
            return retVal;
        }

        internal void Calculate()
        {
            ttList.Clear();

            Double c2 = Temp0 - TempM;
            Double k = Math.Log((Temp1 - TempM) / (Temp0 - TempM)) / time1;

            // //  by time...
            //for (int i = 0; i < 100; i++)
            //{
            //    TempTimeViewModel ttvm = new TempTimeViewModel();
            //    long currentTime = i * 1000;
            //    Double currentTemp = CoolingFunction(TempM, c2, k, currentTime);
            //    TempTime tt = new TempTime( currentTemp, currentTime);
            //    ttvm.TempTime = tt;
            //    ttList.Add(ttvm);
            //}

            // by temp...
            Double currentTemp = Temp0;
            while (currentTemp > TempM)
            {
                long currentTime = InvCoolingFunction(currentTemp, TempM, c2, k);
                TempTimeViewModel ttvm = new TempTimeViewModel();
                TempTime tt = new TempTime( currentTemp, currentTime);
                ttvm.TempTime = tt;
                ttList.Add(ttvm);
                currentTemp -= 0.02;
            }


        }

        private long InvCoolingFunction( Double T, Double Tm, Double c2, Double k )
        {
            return Convert.ToInt64(Math.Log((T - Tm) / c2) / k);
        }

        private Double CoolingFunction( Double Tm, Double c2, Double k, long t )
        {
            return Tm + c2 * Math.Exp(k * t);
        }
    }


    
}
