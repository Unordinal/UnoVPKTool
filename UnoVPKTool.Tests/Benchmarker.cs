using System;
using System.Diagnostics;
using System.Threading;

namespace UnoVPKTool.Tests
{
    public class Benchmarker
    {
        public static void BenchmarkTime(Action action, int iterations = 10000)
        {
            Benchmark<TimeWatch>(action, iterations);
        }

        public static void BenchmarkCpu(Action action, int iterations = 10000)
        {
            Benchmark<CpuWatch>(action, iterations);
        }

        private static void Benchmark<T>(Action action, int iterations) where T : IStopwatch, new()
        {
            //clean Garbage
            GC.Collect();

            //wait for the finalizer queue to empty
            GC.WaitForPendingFinalizers();

            //clean Garbage
            GC.Collect();

            //warm up
            action();

            var stopwatch = new T();
            var timings = new double[5];
            for (int i = 0; i < timings.Length; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                for (int j = 0; j < iterations; j++)
                    action();
                stopwatch.Stop();
                timings[i] = stopwatch.Elapsed.TotalMilliseconds;
            }
            Console.WriteLine("normalized mean: " + MeanAbsoluteDeviation(timings).ToString());
        }

        private static double MeanAbsoluteDeviation(double[] values)
        {
            double sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                double d = values[i];
                sum += d;
            }
            double meanValue = sum / values.Length;

            double dev = 0, dx = 0;
            for (int i = 0; i < values.Length; i++)
            {
                dx = values[i] - meanValue;
                dev += Math.Abs(dx);
            }

            return dev / values.Length;
        }

        private interface IStopwatch
        {
            bool IsRunning { get; }
            TimeSpan Elapsed { get; }

            void Start();

            void Stop();

            void Reset();
        }

        private class TimeWatch : IStopwatch
        {
            private Stopwatch stopwatch = new Stopwatch();

            public TimeSpan Elapsed
            {
                get { return stopwatch.Elapsed; }
            }

            public bool IsRunning
            {
                get { return stopwatch.IsRunning; }
            }

            public TimeWatch()
            {
                if (!Stopwatch.IsHighResolution)
                    throw new NotSupportedException("Your hardware doesn't support high resolution counter");

                //prevent the JIT Compiler from optimizing Fkt calls away
                long seed = Environment.TickCount;

                //use the second Core/Processor for the test
                Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2);

                //prevent "Normal" Processes from interrupting Threads
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

                //prevent "Normal" Threads from interrupting this thread
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }

            public void Start()
            {
                stopwatch.Start();
            }

            public void Stop()
            {
                stopwatch.Stop();
            }

            public void Reset()
            {
                stopwatch.Reset();
            }
        }

        private class CpuWatch : IStopwatch
        {
            private TimeSpan startTime;
            private TimeSpan endTime;
            private bool isRunning;

            public TimeSpan Elapsed
            {
                get
                {
                    if (IsRunning)
                        throw new NotImplementedException("Getting elapsed span while watch is running is not implemented");

                    return endTime - startTime;
                }
            }

            public bool IsRunning
            {
                get { return isRunning; }
            }

            public void Start()
            {
                startTime = Process.GetCurrentProcess().TotalProcessorTime;
                isRunning = true;
            }

            public void Stop()
            {
                endTime = Process.GetCurrentProcess().TotalProcessorTime;
                isRunning = false;
            }

            public void Reset()
            {
                startTime = TimeSpan.Zero;
                endTime = TimeSpan.Zero;
            }
        }
    }
}