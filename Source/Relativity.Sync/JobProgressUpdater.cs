using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
	internal sealed class JobProgressUpdater : IJobProgressUpdater
	{
		private readonly int _workspaceArtifactId;
		private readonly int _jobHistoryArtifactId;
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
		private readonly IAPILog _logger;

		public JobProgressUpdater(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IRdoGuidConfiguration rdoGuidConfiguration, int workspaceArtifactId, int jobHistoryArtifactId, IAPILog logger)
		{
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
			_rdoGuidConfiguration = rdoGuidConfiguration;
			_workspaceArtifactId = workspaceArtifactId;
			_jobHistoryArtifactId = jobHistoryArtifactId;
			_logger = logger;
		}

		public async Task SetTotalItemsCountAsync(int totalItemsCount)
		{
			try
			{
				using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
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
				using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
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
