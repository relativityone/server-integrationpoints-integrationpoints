using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class JobProgressUpdater : IJobProgressUpdater
	{
		private readonly int _workspaceArtifactId;
		private readonly int _jobHistoryArtifactId;
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
		private readonly ISyncLog _logger;

		public JobProgressUpdater(ISourceServiceFactoryForAdmin serviceFactory, IRdoGuidConfiguration rdoGuidConfiguration, int workspaceArtifactId, int jobHistoryArtifactId, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_rdoGuidConfiguration = rdoGuidConfiguration;
			_workspaceArtifactId = workspaceArtifactId;
			_jobHistoryArtifactId = jobHistoryArtifactId;
			_logger = logger;
		}

		public async Task SetTotalItemsCountAsync(int totalItemsCount)
		{
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					UpdateRequest updateRequest = new UpdateRequest()
					{
						Object = new RelativityObjectRef()
						{
							ArtifactID = _jobHistoryArtifactId
						},
						FieldValues = new []
						{
							new FieldRefValuePair()
							{
								Field = new FieldRef()
								{
									Guid = _rdoGuidConfiguration.JobHistory.TotalItemsFieldGuid
								},
								Value = totalItemsCount
							}
						}
					};
					await objectManager.UpdateAsync(_workspaceArtifactId, updateRequest).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update total items count for job history artifact ID: {_jobHistoryArtifactId}. See inner exception for more details.", _jobHistoryArtifactId);
			}
		}

		public async Task UpdateJobProgressAsync(int completedRecordsCount, int failedRecordsCount)
		{
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					UpdateRequest updateRequest = new UpdateRequest()
					{
						Object = new RelativityObjectRef()
						{
							ArtifactID = _jobHistoryArtifactId
						},
						FieldValues = new[]
						{
							new FieldRefValuePair()
							{
								Field = new FieldRef()
								{
									Guid = _rdoGuidConfiguration.JobHistory.CompletedItemsFieldGuid
								},
								Value = completedRecordsCount
							},
							new FieldRefValuePair()
							{
								Field = new FieldRef()
								{
									Guid = _rdoGuidConfiguration.JobHistory.FailedItemsFieldGuid
								},
								Value = failedRecordsCount
							}, 
						}
					};
					await objectManager.UpdateAsync(_workspaceArtifactId, updateRequest).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update items transferred and items failed for job history artifact ID: {_jobHistoryArtifactId}. See inner exception for more details.", _jobHistoryArtifactId);
			}
		}
	}
}