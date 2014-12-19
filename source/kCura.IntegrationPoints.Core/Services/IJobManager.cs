using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Services
{
	public enum TaskType
	{
		None,
		SyncManager,
		SyncWorker
	}

	public interface IJobManager
	{
		void CreateJob<T>(T jobDetails, TaskType task, int integrationPointID, IScheduleRule rule);
		void CreateJob<T>(T jobDetails, TaskType task, int integrationPointID);
		//void CreateJob<T>(T jobDetails, TaskType task); //schedule rules
	}
}
