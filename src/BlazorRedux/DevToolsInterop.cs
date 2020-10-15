using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorRedux
{
    public class DevToolsInterop
    {
        private static readonly object SyncRoot = new object();
        private static bool _isReady;
        private static readonly Queue<Tuple<string, string>> Q = new Queue<Tuple<string, string>>();
        [Inject] private  IJSInProcessRuntime JsInProcessRuntime { get; }
        
        public event EventHandler Reset;
        public event StringEventHandler TimeTravel;
        
        private void OnReset(EventArgs e)
        {
            var handler = Reset;
            handler?.Invoke(null, e);
        }

        private void OnTimeTravel(StringEventArgs e)
        {
            var handler = TimeTravel;
            handler?.Invoke(null, e);
        }

        public void DevToolsReady()
        {
            lock (SyncRoot)
            {
                while (Q.Any())
                {
                    var entry = Q.Dequeue();
                    LogToJs(entry.Item1, entry.Item2);
                }
            }

            _isReady = true;
        }

        public  void DevToolsReset()
        {
            OnReset(new EventArgs());
        }

        public void TimeTravelFromJs(string state)
        {
            OnTimeTravel(new StringEventArgs(state));
        }

        public  void Log(string action, string state)
        {
            if (!_isReady)
            {
                lock (SyncRoot)
                {
                    Q.Enqueue(new Tuple<string, string>(action, state));
                }
            }
            else
            {
                LogToJs(action, state);
            }
        }

         void LogToJs(string action, string state)
        {
            JsInProcessRuntime?.Invoke<bool>("Blazor.log", action, state);
        }
    }
}
