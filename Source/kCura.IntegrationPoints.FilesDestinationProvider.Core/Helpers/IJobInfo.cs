
using System;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public interface IJobInfo
    {
        DateTime GetStartTimeUtc();

        string GetName();
    }
}
