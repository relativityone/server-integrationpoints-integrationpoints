using System;

namespace Relativity.Sync.Executors
{
    internal interface ISleeperWrapper
    {
        void ThreadSleep(TimeSpan sleepDuration);
    }
}
