using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspirationEngine.WPF.Controls
{
    public class TimeSpanEdit : NumericEdit<TimeSpan>
    {
        public TimeSpanEdit()
        {
            Initialized += (s, e) =>
            {
                if (IncrementValue == new TimeSpan())
                    IncrementValue = TimeSpan.FromSeconds(1);
            };
        }
    }
}
