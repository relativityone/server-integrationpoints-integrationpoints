using System;

namespace kCura.IntegrationPoints.Data
{
    [Flags]
    public enum StopState
    {
        None = 0,
        Stopping = 1 << 0,
        Unstoppable = 1 << 1,
        DrainStopping = 1 << 2,
        DrainStopped = 1 << 3
    }
}
