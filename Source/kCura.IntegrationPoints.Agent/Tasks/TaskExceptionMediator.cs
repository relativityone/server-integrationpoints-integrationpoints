using System;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class TaskExceptionMediator : ITaskExceptionMediator, IDisposable
    {
        private readonly ITaskExceptionService _taskExceptionService;
        private Agent _agent;

        public TaskExceptionMediator(ITaskExceptionService taskExceptionService)
        {
            _taskExceptionService = taskExceptionService;
        }

        public void RegisterEvent(ScheduleQueueAgentBase agent)
        {
            if (agent is Agent)
            {
                _agent = agent as Agent;
                _agent.JobExecutionError += OnJobExecutionError;
            }
        }

        private void OnJobExecutionError(Job job, ITask task, Exception exception)
        {
            if (task != null)
            {
                _taskExceptionService.EndTaskWithError(task, exception);
            }
            else if (job != null)
            {
                _taskExceptionService.EndJobWithError(job, exception);
            }
        }

        public void Dispose()
        {
            if (_agent != null)
            {
                _agent.JobExecutionError -= OnJobExecutionError;
            }
        }
    }
}
