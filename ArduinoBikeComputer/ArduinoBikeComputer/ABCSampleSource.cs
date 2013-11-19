using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArduinoBikeComputer
{
    public interface IABCSampleSource
    {
        event Action<ABCSample> SampleArrived;

    }
}
