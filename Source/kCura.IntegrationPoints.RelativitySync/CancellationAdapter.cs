using System;
using System.Threading;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class CancellationAdapter
	{
		public static CancellationToken GetCancellationToken(Job job, IWindsorContainer ripContainer)
		{
			IManagerFactory managerFactory = ripContainer.Resolve<IManagerFactory>();
			IJobService jobService = ripContainer.Resolve<IJobService>();
			IJobHistoryService jobHistoryService = ripContainer.Resolve<IJobHistoryService>();

			Guid jobIdentifier;

			if (string.IsNullOrWhiteSpace(job.JobDetails))
			{
				jobIdentifier = Guid.NewGuid();
			}
			else
			{
				ISerializer serializer = ripContainer.Resolve<ISerializer>();
				TaskParameters taskParameters = serializer.Deserialize<TaskParameters>(job.JobDetails);
				jobIdentifier = taskParameters.BatchInstance;
			}

			IJobStopManager jobStopManager = managerFactory.CreateJobStopManager(jobService, jobHistoryService, jobIdentifier, job.JobId, true);
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			jobStopManager.StopRequestedEvent += (sender, args) => tokenSource.Cancel();
			return tokenSource.Token;
		}
	}
}