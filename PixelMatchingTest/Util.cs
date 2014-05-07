using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelMatchingTest
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
    }
}
