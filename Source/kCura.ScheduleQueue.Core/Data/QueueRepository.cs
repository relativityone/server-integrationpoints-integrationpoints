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

        public QueueRepository(IQueueDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public long AddJob(Job job)
        {
            long jobId = 0;

            using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
            {
                jobId = connection.Insert(job);
            }

            return jobId;
        }

        public IList<Job> GetAllJobs()
        {
            List<Job> jobs = null;

            using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
            {
                jobs = connection.GetAll<Job>().ToList();
            }

            return jobs;
        }

        public Job GetJob(long jobId)
        {
            Job job = null;

            using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
            {
                job = connection.Get<Job>(jobId);
            }

            return job;
        }

        public bool UpdateJob(Job job)
        {
            bool updated = false;

            using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
            {
                updated = connection.Update(job);
            }

            return updated;
        }

        public bool DeleteJob(long jobId)
        {
            bool deleted = false;

            using (SqlConnection connection = _dbContext.EddsDBContext.GetConnection())
            {
                deleted = connection.Delete(new Job { JobId = jobId });
            }

            return deleted;
        }
    }
}
