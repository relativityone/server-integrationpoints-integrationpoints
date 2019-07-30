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
using kCura.IntegrationPoints.RelativitySync.RipOverride;
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

		public async Task<ExecutionResult> ExecuteAsync(INotificationConfiguration configuration, CancellationToken token)
		{
			// Email notification is being handled by IBatchStatus which is being run by ExportServiceManager.
			// We don't want to double the emails.
			return await Task.FromResult(ExecutionResult.Success()).ConfigureAwait(false);
		}
	}
}