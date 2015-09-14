using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PlayingWithPerformanceCounters
{
    public static class Program
    {
        private const string CounterCategory = "MBSI Applications";
        private static PerformanceCounter _totalOperations;
        private static PerformanceCounter _operationsPerSecond;
        private static PerformanceCounter _averageDuration;
        private static PerformanceCounter _averageDurationBase;
        //private static DateTime _startTime;
        private static readonly Random Rand = new Random();
        private static long _startTimeTicks;
        private static long _endTimeTicks;

        static void Main()
        {
            //_startTime = DateTime.Now;

            CreateCounters();
            SetupCounters();

            _totalOperations.RawValue = 0;

            for (var i = 0; i < 1000; i++)
            {
                QueryPerformanceCounter(ref _startTimeTicks);
                System.Threading.Thread.Sleep(Rand.Next(1000));
                QueryPerformanceCounter(ref _endTimeTicks);
                UseCounters(_endTimeTicks - _startTimeTicks);
            }            
        }

        private static void CreateCounters()
        {
            // If this crashes with a "Cannot load Counter Name data because an invalid index '' was read from the registry."
            // exception then open cmd as an administrator and run the following
            // lodctr /r
            // This refreshes the counter cache in the registry
            // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Perflib\009 \ Counter+Help

            if (PerformanceCounterCategory.Exists(CounterCategory))
            {
                //PerformanceCounterCategory.Delete(CounterCategory);
                return;
            }

            var counters = new CounterCreationDataCollection();

            var totalOps = new CounterCreationData
            {
                CounterName = "# operations executed",
                CounterHelp = "Total number of operations executed",
                CounterType = PerformanceCounterType.NumberOfItems32
            };
            counters.Add(totalOps);

            var opsPerSecond = new CounterCreationData
            {
                CounterName = "# operations /sec",
                CounterHelp = "Number of operations executed per second",
                CounterType = PerformanceCounterType.RateOfCountsPerSecond32
            };
            counters.Add(opsPerSecond);

            var avgDuration = new CounterCreationData
            {
                CounterName = "average time per operation",
                CounterHelp = "Average duration per operation execution",
                CounterType = PerformanceCounterType.AverageTimer32
            };
            counters.Add(avgDuration);

            var avgDurationBase = new CounterCreationData
            {
                CounterName = "average time per operation base",
                CounterHelp = "Average duration per operation execution base",
                CounterType = PerformanceCounterType.AverageBase
            };
            counters.Add(avgDurationBase);

            PerformanceCounterCategory.Create(CounterCategory, "MBSI category", PerformanceCounterCategoryType.Unknown, counters);
        }

        private static void SetupCounters()
        {
            // create counters to work with
            _totalOperations = new PerformanceCounter
            {
                CategoryName = CounterCategory,
                CounterName = "# operations executed",
                MachineName = ".",
                ReadOnly = false
            };

            _operationsPerSecond = new PerformanceCounter
            {
                CategoryName = CounterCategory,
                CounterName = "# operations / sec",
                MachineName = ".",
                ReadOnly = false
            };

            _averageDuration = new PerformanceCounter
            {
                CategoryName = CounterCategory,
                CounterName = "average time per operation",
                MachineName = ".",
                ReadOnly = false
            };

            _averageDurationBase = new PerformanceCounter
            {
                CategoryName = CounterCategory,
                CounterName = "average time per operation base",
                MachineName = ".",
                ReadOnly = false
            };
        }

        private static void UseCounters(long ticks)
        {
            //var dtTicks = (DateTime.Now - _startTime).Ticks;
            var op = Rand.Next(5);

            _totalOperations.Increment();
            _operationsPerSecond.IncrementBy(op);
            
            Console.WriteLine(op);
            _averageDuration.IncrementBy(ticks); // or dtTicks
            _averageDurationBase.Increment();
            //_startTime = DateTime.Now;
        }

        [DllImport("Kernel32.dll")]
        private static extern void QueryPerformanceCounter(ref long ticks);
    }
}
