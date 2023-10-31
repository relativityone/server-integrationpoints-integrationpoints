using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.Core.Exceptions;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.BatchProcess
{
    public delegate void JobPreExecuteEvent(Job job, TaskResult taskResult);

    public delegate void JobPostExecuteEvent(Job job, TaskResult taskResult, long items);

    public abstract class BatchManagerBase<T> : ITask where T : class
    {
        private readonly IAPILog _logger;

        protected IDiagnosticLog DiagnosticLog { get; }

        protected BatchManagerBase(IHelper helper, IDiagnosticLog diagnosticLog)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<BatchManagerBase<T>>();
            BatchSize = 1000;

            DiagnosticLog = diagnosticLog;
        }

        public virtual int BatchSize { get; }

        public virtual void Execute(Job job)
        {
            LogExecuteStart(job);

            TaskResult taskResult = new TaskResult();
            long items = 0;
            try
            {
                OnRaiseJobPreExecute(job, taskResult);
                items = BatchTask(job, GetUnbatchedIDs(job));
                taskResult.Status = TaskStatusEnum.Success;
                LogExecuteSuccesfulEnd(job);
            }
            catch (OperationCanceledException e)
            {
                taskResult.Status = TaskStatusEnum.Success;
                LogStoppingJob(e);

                // DO NOTHING. Someone attempted to stop the job.
            }
            catch (Exception e)
            {
                taskResult.Status = TaskStatusEnum.Fail;
                taskResult.Exceptions = new List<Exception> { e };
                LogJobFailed(e);
                throw;
            }
            finally
            {
                OnRaiseJobPostExecute(job, taskResult, items);
                LogExecuteFinalize(job);
            }
        }

        public event JobPreExecuteEvent RaiseJobPreExecute;

        public event JobPostExecuteEvent RaiseJobPostExecute;

        public abstract IEnumerable<T> GetUnbatchedIDs(Job job);

        public virtual long BatchTask(Job job, IEnumerable<T> batchIDs)
        {
            long count = 0;
            long startIndex = 0;
            var list = new List<T>();
            foreach (var id in batchIDs)
            {
                // TODO: later we will need to generate error entry for every item we bypass
                if ((id != null) && id is string && (id.ToString() != string.Empty))
                {
                    list.Add(id);
                    count += 1;
                    DiagnosticLog.LogDiagnostic("In BatchTask - {count}, ID: {id}", count, id);
                    if (list.Count == BatchSize)
                    {
                        CreateBatchJob(job, list, startIndex);
                        list = new List<T>();
                        startIndex = count;
                    }
                }
                else
                {
                    LogMissingIdError(count);
                }
            }
            if (list.Any())
            {
                CreateBatchJob(job, list, startIndex);
            }
            return count;
        }

        public abstract void CreateBatchJob(Job job, List<T> batchIDs, long startingIndex);

        protected virtual void OnRaiseJobPreExecute(Job job, TaskResult taskResult)
        {
            if (RaiseJobPreExecute != null)
            {
                LogRaisePreExecute(job);
                RaiseJobPreExecute(job, taskResult);
            }
        }

        protected virtual void OnRaiseJobPostExecute(Job job, TaskResult taskResult, long items)
        {
            if (RaiseJobPostExecute != null)
            {
                LogRaisePostExecute(job);
                RaiseJobPostExecute(job, taskResult, items);
                if (job.JobFailed.Exception != null && job.JobFailed.Exception.GetType() == typeof(ScheduleRunTimeGenerationException))
                {
                    throw job.JobFailed.Exception;
                }
            }
        }

        #region Logging

        private void LogJobFailed(Exception e)
        {
            _logger.LogError(e, "Failed to execute job");
        }

        private void LogStoppingJob(OperationCanceledException e)
        {
            _logger.LogInformation(e, "Someone attempted to stop the job");
        }

        private void LogExecuteFinalize(Job job)
        {
            _logger.LogInformation("Batch Manager Base: Finalizing execution of job: {JobId}", job.JobId);
        }

        private void LogExecuteSuccesfulEnd(Job job)
        {
            _logger.LogInformation("Batch Manager Base: Succesfully executed job: {JobId}", job.JobId);
        }

        private void LogExecuteStart(Job job)
        {
            _logger.LogInformation("Batch Manager Base: Started execution of job: {JobId}", job.JobId);
        }

        private void LogRaisePostExecute(Job job)
        {
            _logger.LogInformation("Batch Manager Base: Raising post execute event for job: {JobId}", job.JobId);
        }

        private void LogRaisePreExecute(Job job)
        {
            _logger.LogInformation("Batch Manager Base: Raising pre execute event for job: {JobId}", job.JobId);
        }

        private void LogMissingIdError(long count)
        {
            _logger.LogError("One of the items has invalid id and will not be processed. It will not be included in batch. Current count in the batch is {count}. Stepping over to next item.", count);
        }

        #endregion
    }
}
