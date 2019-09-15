using System;
using System.Diagnostics;

namespace Hvt.VrrGtfsTransformer.Extensions
{
    public static class StopwatchExtensions
    {
        public static TimeSpan StopAndMeasure(this Stopwatch stopwatch)
        {
            stopwatch.Stop();
            TimeSpan result = stopwatch.Elapsed;
            stopwatch.Reset();
            return result;
        }
    }
}
