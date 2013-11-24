using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArduinoBikeComputer
{
    public interface IABCSampleSource
    {
        event Action<ABCSample> SampleArrived;

        //event EventHandler SampleArrived;

    }

    //public class ABCSampleArriveEventArgs : EventArgs
    //{
    //    public ABCSample Sample { get; set; }
    //}
}
