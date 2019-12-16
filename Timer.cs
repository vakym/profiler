using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.Timers
{
    public static class Timer
    {
        private readonly static string template = new string(' ', 16);
        public static string Report
        {
            get
            {
                StringBuilder report = new StringBuilder();
                foreach(var counter in counters)
                {
                    report.Append($"{new string(' ', counter.Item1 * 4)}" +
                        $"{counter.Item2.ClockName}" +
                        $"{new string(' ', 20 - (counter.Item1 * 4 + counter.Item2.ClockName.Length))}" +
                        $": {counter.Item2.ElapsedMilliseconds}\n");
                }
                counters.Clear();
                return report.ToString();
            }
        }
        private static ClockPool clockPool = new ClockPool(10);
        private static List<Tuple<int, Clock>> counters = new List<Tuple<int, Clock>>();
        public static Clock Start(string timerName = "*")
        {
            var clock = clockPool.SendToPool(timerName);
            counters.Add(Tuple.Create(ClockPool.StartedClockCount-1, clock));
            return clock;
        }
    }


    public class ClockPool
    {
        public static int StartedClockCount { get; private set; } = 0;

        private List<Clock> clocks = new List<Clock>();
        private int freeClockIndex = 0;
        public ClockPool(int sizeOfPool)
        {
            for (int i = 0; i < sizeOfPool; i++)
            {
                clocks.Add(CreateClock());
            }
        }
          
        public Clock SendToPool(string name)
        {
            clocks[freeClockIndex].ClockName = name;
            clocks[freeClockIndex].Start();
            freeClockIndex++;
            return clocks[freeClockIndex-1];
        }
        public static Clock CreateClock()
        {
            var clock = new Clock();
            clock.ClockStateChanged += Clock_ClockStateChanged;
            return clock;
        }

        private static void Clock_ClockStateChanged(object sender, bool isClockStarted)
        {
            if (isClockStarted)
                StartedClockCount++;
            else
                StartedClockCount--;
        }
    }

    public class Clock : Stopwatch,IDisposable
    {
        public string ClockName { get; set; }

        public event EventHandler<bool> ClockStateChanged;


        public new void Stop()
        {
            ClockStateChanged(this, false);
            base.Stop();
        }

        public new void Start()
        {
            ClockStateChanged(this, true);
            base.Start();
        }
        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты).
                }
                Start();
                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                // TODO: задать большим полям значение NULL.
               
                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
        // ~Clock()
        // {
        //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
        //   Dispose(false);
        // }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            Dispose(true);
            // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
