using System;

namespace kCura.IntegrationPoints.Common.Helpers
{
    public interface ITimer : IDisposable
    {
        bool Change(int dueTime, int period);
    }
}
