using System;
using System.Threading;

namespace Relativity.Sync.Executors
{
    internal class SleeperWrapper : ISleeperWrapper
    {
        public void ThreadSleep(TimeSpan sleepDuration)
        {
            Thread.Sleep(sleepDuration);
        }
    }
}
