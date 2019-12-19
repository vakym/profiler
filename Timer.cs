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
        public static string Report { get => GenerateReport(); }
        public static TimerHolder Start(string name="*")
        {
            var tickCounter = new TimerHolder(name);
            tickCounter.Start();
            TimerHandles.Add(tickCounter);
            return tickCounter;
        }

        private static List<TimerHolder> TimerHandles = new List<TimerHolder>(10);
        
        private static string GenerateReport()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var timer in TimerHandles)
            {
                var timerGeneration = GetTimerGeneration(timer);
                var startSpaceCount = timerGeneration * 4;
                stringBuilder.Append(new string(' ', startSpaceCount));
                stringBuilder.Append(timer.Name);
                stringBuilder.Append(new string(' ',20 - timer.Name.Length+ startSpaceCount));
                stringBuilder.Append($": {timer.ElapsedMilliseconds}\n");
            }
            return stringBuilder.ToString();
        }

        public static int GetTimerGeneration(TimerHolder timer)
        {
            if (!TimerHandles.Contains(timer))
                return 0;
            int generation = 0;
            for (int i = 0; i < TimerHandles.Count; i++)
            {
                if (ReferenceEquals(TimerHandles[i], timer))
                    break;
                if (TimerHandles[i].IsRunning || TimerHandles[i].EndTick >= timer.StartTick)
                    generation++; 
            }
            return generation;
        }
    }

    public class TimerHolder : IDisposable
    {
        public string Name { get; }

        public long StartTick { get; private set; }

        public long EndTick { get; private set; }

        public long ElapsedTicks
        {
            get => ConvertToDateTimeTicks(EndTick - StartTick);
        }

        public double ElapsedMilliseconds
        {
            //Если делить с дробной частью, то можно засекать десятые и соты милисикунды,
            //но тогда не проходит тесты
            get => ElapsedTicks / TicksPerMilliseconds;
        }

        public bool IsRunning { get; private set; } = false;

        public void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                StartTick = GetTicksCount();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                EndTick = GetTicksCount();
                IsRunning = false;
            }
        }

        public TimerHolder(string name)
        {
            Name = name;
        }
        
        private long GetTicksCount()
        {
           return Stopwatch.GetTimestamp();
        }

        private long ConvertToDateTimeTicks(long elapsedRawTicks)
        {
            if (Stopwatch.IsHighResolution)
            {
                ///Если в системе есть HPET( https://ru.wikipedia.org/wiki/HPET ),
                ///то тики StopWatch != тикам DateTime и зависят от частоты в HPET(Stopwatch.Frequency)
                double dticks = elapsedRawTicks;
                dticks *=  TicksPerSecond / Stopwatch.Frequency;
                return unchecked((long)dticks);
            }
            else
            {
                return elapsedRawTicks;
            }
        }
        private const long TicksPerMilliseconds = 10000;
        private const long TicksPerSecond = TicksPerMilliseconds * 1000;
        #region IDisposable Support
        private bool disposedValue = false; 
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты).
                }
                Stop();
                disposedValue = true;
            }
        }

        ~TimerHolder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
