using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Data;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	/// <summary>
	///     Untestable code. References to static configuration classes and SMPT server
	/// </summary>
	internal sealed class Notification : IExecutor<INotificationConfiguration>, IExecutionConstrains<INotificationConfiguration>
	{
		private readonly IWindsorContainer _container;

		public Notification(IWindsorContainer container)
		{
			_container = container;
		}

		public Task<bool> CanExecuteAsync(INotificationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(configuration.SendEmails);
		}

		public async Task ExecuteAsync(INotificationConfiguration configuration, CancellationToken token)
		{
			await Task.Yield();

			KeywordConverter converter = _container.Resolve<KeywordConverter>();
			ISendEmailWorker sendEmailWorker = _container.Resolve<ISendEmailWorker>();
			IExtendedJob extendedJob = _container.Resolve<IExtendedJob>();
			IJobHistoryService jobHistoryService = _container.Resolve<IJobHistoryService>();

			IList<JobHistory> history = jobHistoryService.GetJobHistory(new[] {extendedJob.JobHistoryId});

			if (history.Count != 1)
			{
				throw new ArgumentException($"Unable to find JobHistory {extendedJob.JobHistoryId}");
			}

			EmailMessage emailMessage = BatchEmail.GenerateEmail(history[0].JobStatus);
			BatchEmail.ConvertMessage(emailMessage, configuration.EmailRecipients, converter);

			sendEmailWorker.Execute(emailMessage, extendedJob.JobId);
		}
	}
}