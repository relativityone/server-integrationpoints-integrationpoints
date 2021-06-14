using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal sealed class BatchRepository : IBatchRepository
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly IDateTime _dateTime;

		public BatchRepository(ISourceServiceFactoryForAdmin serviceFactory, IDateTime dateTime)
		{
			_serviceFactory = serviceFactory;
			_dateTime = dateTime;
		}

		public Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			return Batch.CreateAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, totalItemsCount, startingIndex);
		}

		public Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId)
		{
			return Batch.GetAsync(_serviceFactory, workspaceArtifactId, artifactId);
		}

		public Task<IEnumerable<IBatch>> GetAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			return Batch.GetAllAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId);
		}

		public async Task DeleteAllForConfigurationAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new MassDeleteByCriteriaRequest
				{
					ObjectIdentificationCriteria = new ObjectIdentificationCriteria()
					{
						ObjectType = new ObjectTypeRef
						{
							Guid = Batch.BatchObjectTypeGuid
						},
						Condition = $"'{Batch._PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}"
					}
				};
				await objectManager.DeleteAsync(workspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		public async Task DeleteAllOlderThanAsync(int workspaceArtifactId, TimeSpan olderThan)
		{
			IEnumerable<int> oldConfiguratiosArtifactIds = await GetConfigurationsOlderThanAsync(_serviceFactory, _dateTime, workspaceArtifactId, olderThan).ConfigureAwait(false);
			IEnumerable<Task> deleteTasks = oldConfiguratiosArtifactIds.Select(configurationArtifactId => DeleteAllForConfigurationAsync(workspaceArtifactId, configurationArtifactId));
			await Task.WhenAll(deleteTasks).ConfigureAwait(false);
		}

		public Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Batch.GetLastAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Batch.GetAllBatchesIdsToExecuteAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IEnumerable<IBatch>> GetAllSuccessfullyExecuteBatchesAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Batch.GetAllSuccessfullyExecuteBatchesAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			return Batch.GetNextAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, startingIndex);
		}
		
		private static async Task<IEnumerable<int>> GetConfigurationsOlderThanAsync(ISourceServiceFactoryForAdmin serviceFactory, IDateTime dateTime, int workspaceArtifactId, TimeSpan olderThan)
		{
			DateTime createdBeforeDate = dateTime.UtcNow - olderThan;
			DateTime GetObjectCreationDate(RelativityObject relativityObject) => (DateTime)relativityObject.FieldValues.Single().Value;

			using (IObjectManager objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Guid = new Guid(SyncRdoGuids.SyncConfigurationGuid)
					},
					Fields = new[]
					{
						new FieldRef()
						{
							Name = "System Created On"
						}
					},
					IncludeNameInQueryResult = true
				};

				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, request, 0, int.MaxValue).ConfigureAwait(false);
				IEnumerable<RelativityObject> oldConfigurations = queryResult.Objects.Where(configurationObject =>
					GetObjectCreationDate(configurationObject) < createdBeforeDate);
				return oldConfigurations.Select(x => x.ArtifactID);
			}
		}
	}
}