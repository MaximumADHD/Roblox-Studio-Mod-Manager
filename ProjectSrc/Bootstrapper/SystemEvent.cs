using System;
using System.Threading;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class SystemEvent : EventWaitHandle
    {
        public string Name { get; private set; }
        public bool FireEvent() => Set();

        public SystemEvent(string name, bool init = false, EventResetMode mode = EventResetMode.AutoReset) : base(init, mode, name)
        {
            if (init)
                Reset();
            else
                Set();

            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public Task<bool> WaitForEvent()
        {
            return Task.Factory.StartNew(WaitOne);
        }

        public Task<bool> WaitForEvent(TimeSpan timeout, bool exitContext = false)
        {
            return Task.Factory.StartNew(() => WaitOne(timeout, exitContext));
        }

        public Task<bool> WaitForEvent(int millisecondsTimeout, bool exitContext = false)
        {
            return Task.Factory.StartNew(() => WaitOne(millisecondsTimeout, exitContext));
        }
    }
}
