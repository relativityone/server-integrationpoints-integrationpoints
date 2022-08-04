using System;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Context
{
    public class JobContextProvider : IJobContextProvider, IDisposable
    {
        private Job _job;

        public bool IsContextStarted => _job != null;

        public IDisposable StartJobContext(Job job)
        {
            if (IsContextStarted)
            {
                throw new InvalidOperationException("Starting new job context before previous context was disposed");
            }

            _job = job;

            return this;
        }

        public Job Job
        {
            get
            {
                if (_job == null)
                {
                    throw new InvalidOperationException("Job is not present because context wasn't initialized");
                }
                return _job;
            }
        }

        public void Dispose()
        {
            _job = null;
        }
    }
}
