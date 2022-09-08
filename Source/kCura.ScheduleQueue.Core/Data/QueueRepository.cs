using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Contrib.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Data.Interfaces;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
    public class QueueRepository : IQueueRepository
    {
        private readonly IQueueDBContext _dbContext;
        private readonly IAPILog _logger;

        public QueueRepository(IQueueDBContext dbContext, IAPILog logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public long AddJob(Job job)
        {
            long jobId = 0;
            try
            {
                using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
                {
                    jobId = connection.Insert(job);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not add job to ScheduleAgentQueue");
            }

            return jobId;
        }

        public IList<Job> GetAllJobs()
        {
            List<Job> jobs = null;
            try
            {
                using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
                {
                    jobs = connection.GetAll<Job>().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get all jobs from ScheduleAgentQueue");
            }

            return jobs;
        }

        public Job GetJob(long jobId)
        {
            Job job = null;

            try
            {
                using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
                {
                    job = connection.Get<Job>(jobId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get job ID={id} from ScheduleAgentQueue", jobId);
            }

            return job;
        }

        public bool UpdateJob(Job job)
        {
            bool updated = false;
            try
            {
                using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
                {
                    updated = connection.Update(job);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not update job ID={id} from ScheduleAgentQueue", job.JobId);
            }

            return updated;
        }

        public bool DeleteJob(long jobId)
        {
            bool deleted = false;

            try
            {
                using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
                {
                    deleted = connection.Delete(new Job { JobId = jobId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not delete job ID={id} from ScheduleAgentQueue", jobId);
            }

            return deleted;
        }
    }
}
