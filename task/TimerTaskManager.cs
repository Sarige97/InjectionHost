using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace muju.task
{
    internal class TimerTaskManager
    {
        private static readonly Dictionary<int, TimerTask> _timerTaskDict = new Dictionary<int, TimerTask>();
        
        public static void AddTimerEvent(int loopTime, Action action)
        {

            if (_timerTaskDict.TryGetValue(loopTime, out TimerTask timerTask))
            {
                timerTask.AddAction(action);
            } else
            {
                TimerTask newTimeTask = new TimerTask(loopTime);
                newTimeTask.AddAction(action);
                _timerTaskDict.Add(loopTime, newTimeTask);
            }
        }

    }
}
