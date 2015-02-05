using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanning
{
    class Util
    {
        // http://stackoverflow.com/a/6800845
        public static int Min(params int[] values)
        {
            return Enumerable.Min(values);
        }

        public static int Max(params int[] values)
        {
            return Enumerable.Max(values);
        }

        // A simple struct representing a pair of elements, of which one is a minimum and the other
        // is a maximum
        public struct MinMaxPair<T>
        {
            public T Min { get { return min; } }
            public T Max { get { return max; } }

            private readonly T min;
            private readonly T max;

            public MinMaxPair(T min, T max)
            {
                this.min = min;
                this.max = max;
            }
        }
    }
}
