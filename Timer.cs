using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Memory.Timers
{
    public static class Timer
    {
        public static string Report { get => GetReport(); }

        public static TimerHolder Start(string timerName  = "*")
        {
            var parentTimer = lastAddedTimer.TimerSelect(timer => timer != null, s => s.IsRunning); 
            lastAddedTimer = new TimerHolder(timerName, parentTimer);
            if(parentTimer !=null)
                parentTimer.Childrens.Add(lastAddedTimer);
            return lastAddedTimer;
        }

        private static TimerHolder lastAddedTimer;
        private static string GetReport()
        {
            StringBuilder report = new StringBuilder();
            BuildReport(0, lastAddedTimer.TimerSelect(timer=>timer.Parent!=null, s=>false));
            return report.ToString();

            void BuildReport(int startGeneration, TimerHolder rootTimer)
            {
                AddItemToReport(4 * startGeneration, rootTimer.Name, rootTimer.ElapsedMilliseconds);
                long sumOfChildrensMilliseconds = 0;
                foreach (var child in rootTimer.Childrens)
                {
                    sumOfChildrensMilliseconds += child.ElapsedMilliseconds;
                    BuildReport(startGeneration + 1, child);
                }
                if (rootTimer.Childrens.Count>0)
                    AddItemToReport(4 * (startGeneration + 1), "Rest", rootTimer.ElapsedMilliseconds - sumOfChildrensMilliseconds);
            }

            void AddItemToReport(int startSpaces, string name, double value)
            {
                report.Append(new string(' ', startSpaces));
                report.Append(name);
                report.Append(new string(' ', 20 - (name.Length + startSpaces)));
                report.Append($": {value}\n");
            }
        }
    }

    public class TimerHolder : Stopwatch, IDisposable
    {
        public string Name { get; }

        public List<TimerHolder> Childrens { get; } = new List<TimerHolder>();
        
        public TimerHolder Parent { get; }

        public TimerHolder(string name, TimerHolder parentTimerHolder = null)
        {
            Name = name;
            Parent = parentTimerHolder;
            Start(); 
        }

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

    public static class TimerHolderExtensions
    {
        public static TimerHolder TimerSelect(this TimerHolder startTimer,
                                              Func<TimerHolder, bool> selector,
                                              Func<TimerHolder, bool> returnСriterion)
        {
            while (selector(startTimer))
            {
                if (returnСriterion(startTimer))
                    return startTimer;
                startTimer = startTimer.Parent;
            }
            return startTimer;
        }
    }
}
