using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace ABCTempTableGenerator
{
    public class TempTimeList : ObservableCollection<TempTimeViewModel>
    {
        public TempTimeList() : base()
        {

        }

    }


    public class TempTime
    {
        public Double Temp { get; set; }
        public long Time { get; set; }

        public TempTime(Double temp, long time )
        {
            Temp = temp;
            Time = time;
        }
    }

    public class TempTimeViewModel : DependencyObject

    {
        public TempTimeViewModel()
        {
        
        }

        public TempTime TempTime
        {
            set
            {
                this.Temp = value.Temp;
                this.Time = value.Time;
            }
        }


        public Double Temp
        {
            get { return (Double)GetValue(TempProperty); }
            set { SetValue(TempProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Temp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TempProperty =
            DependencyProperty.Register("Temp", typeof(Double), typeof(TempTimeViewModel), new UIPropertyMetadata(0.0d));



        public long Time
        {
            get { return (long)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(long), typeof(TempTimeViewModel), new UIPropertyMetadata(0L));



        //public TempTimeViewModel( Double temp, long time)
        //{
        //    Temp = temp;
        //    Time = time;
        //}

    }
}
