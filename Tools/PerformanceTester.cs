using System;
using System.Diagnostics;
using System.Threading;

namespace Apprentice.Tools {
    public static class PerformanceTester {

        private static bool prepared;

        /// <summary>Create stable environment to achieve consistent results</summary>
        private static PrepInfo Preparation() {
            if (prepared)
                throw new Exception("Cannot run two performance testers at once");
            prepared = true;

            var info = new PrepInfo {
                affinity = Process.GetCurrentProcess().ProcessorAffinity,
                processPriority = Process.GetCurrentProcess().PriorityClass,
                threadPriority = Thread.CurrentThread.Priority
            };

            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2);
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            return info;
        }

        private static void Relaxation(PrepInfo info) {
            Process.GetCurrentProcess().ProcessorAffinity = info.affinity;
            Process.GetCurrentProcess().PriorityClass = info.processPriority;
            Thread.CurrentThread.Priority = info.threadPriority;
            prepared = false;
        }

        public static void TestPrint(Action action) => Console.WriteLine($"Result: {Test(action)} ms");
        public static double Test(Action action) {
            // Set up optimal testing environment
            var info = Preparation();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Warmup testing environment
            while (watch.ElapsedMilliseconds < 1500) {
                action.Invoke();
            }

            watch.Restart();
            action.Invoke();
            watch.Stop();

            // Revert back to normal environment
            Relaxation(info);
            return watch.ElapsedMilliseconds;
        }

        public static void TestPrint(Action action, int repeat) => Console.WriteLine($"Result: {Test(action, repeat)} ms");
        public static double Test(Action action, int repeat) {
            // Set up optimal testing environment
            var info = Preparation();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Warmup testing environment
            while (watch.ElapsedMilliseconds < 1500) {
                action.Invoke();
            }

            watch.Restart();
            for (int i = 0; i < repeat; i++)
                action.Invoke();
            watch.Stop();

            // Revert back to normal environment
            Relaxation(info);
            return watch.ElapsedMilliseconds / (double) repeat;
        }

        public struct PrepInfo {
            public IntPtr affinity;
            public ProcessPriorityClass processPriority;
            public ThreadPriority threadPriority;
        }
    }
}
