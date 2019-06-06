using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
	// TODO REL-292382 This is a temporary solution for updating job history, until we implement REL-292382.
	internal sealed class JobProgressUpdater : IJobProgressUpdater
	{
		private readonly int _workspaceArtifactId;
		private readonly int _jobHistoryArtifactId;
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;

		private static readonly Guid CompletedItemsCountGuid = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c");
		private static readonly Guid FailedItemsCountGuid = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e");
		private static readonly Guid TotalItemsCountGuid = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b");

		public JobProgressUpdater(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int jobHistoryArtifactId)
		{
			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;
			_jobHistoryArtifactId = jobHistoryArtifactId;
		}

		public async Task SetTotalItemsCountAsync(int totalItemsCount)
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
								Guid = TotalItemsCountGuid
							},
							Value = totalItemsCount
						}
					}
				};
				await objectManager.UpdateAsync(_workspaceArtifactId, updateRequest).ConfigureAwait(false);
			}
		}

		public async Task UpdateJobProgressAsync(int completedRecordsCount, int failedRecordsCount)
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
								Guid = CompletedItemsCountGuid
							},
							Value = completedRecordsCount
						},
						new FieldRefValuePair()
						{
							Field = new FieldRef()
							{
								Guid = FailedItemsCountGuid
							},
							Value = failedRecordsCount
						}, 
					}
				};
				await objectManager.UpdateAsync(_workspaceArtifactId, updateRequest).ConfigureAwait(false);
			}
		}
	}
}