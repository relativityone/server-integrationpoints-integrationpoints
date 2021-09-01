using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal sealed class BatchRepository : IBatchRepository
	{
		private readonly IRdoManager _rdoManager;
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly IDateTime _dateTime;

		public BatchRepository(IRdoManager rdoManager,ISourceServiceFactoryForAdmin serviceFactory, IDateTime dateTime)
		{
			_rdoManager = rdoManager;
			_serviceFactory = serviceFactory;
			_dateTime = dateTime;
		}

		public Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalDocumentsCount, int startingIndex)
		{
			return Batch.CreateAsync(_rdoManager, _serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, totalDocumentsCount, startingIndex);
		}

		public Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId)
		{
			return Batch.GetAsync(_rdoManager, _serviceFactory, workspaceArtifactId, artifactId);
		}

		public Task<IEnumerable<IBatch>> GetAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			return Batch.GetAllAsync(_rdoManager, _serviceFactory, workspaceArtifactId, syncConfigurationArtifactId);
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
			return Batch.GetLastAsync(_rdoManager, _serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Batch.GetAllBatchesIdsToExecuteAsync(_rdoManager, _serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Batch.GetAllSuccessfullyExecutedBatchesAsync(_rdoManager, _serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			return Batch.GetNextAsync(_rdoManager, _serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, startingIndex);
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