﻿using kCura.IntegrationPoints.Agent.Tasks;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Sync
{
    internal interface IScheduledSyncTask : ITask, ITaskWithJobHistory
    {
    }
}
