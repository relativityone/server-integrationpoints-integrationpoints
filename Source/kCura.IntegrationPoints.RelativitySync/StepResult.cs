using System;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal sealed class StepResult
    {
        public StepResult(ExecutionStatus executionStatus, TimeSpan duration)
        {
            ExecutionStatus = executionStatus;
            Duration = duration;
        }

        public ExecutionStatus ExecutionStatus { get; }

        public TimeSpan Duration { get; }
    }
}
