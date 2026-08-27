using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace muju.task
{
    internal class TimerTask
    {
        public int LoopTime { get; }

        private readonly List<Action> _actions = new List<Action>();

        private readonly Timer _timer; 

        public TimerTask(int loopTime)
        {
            this.LoopTime = loopTime;
            this._timer = new Timer();
            // 初始化对象时创建定时任务定时执行操作
            this._timer.Interval = loopTime;
            _timer.Tick += (s, e) =>
            {
                foreach(var action in _actions)
                {
                    action.Invoke();
                }
            };
            _timer.Start();
        }

        public void AddAction(Action action) 
        {
            _actions.Add(action);
        }

        public void RemoveAction(Action action)
        {
            _actions.Remove(action);
        }



    }
}
