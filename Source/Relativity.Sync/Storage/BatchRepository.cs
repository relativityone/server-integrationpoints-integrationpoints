using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class BatchRepository : IBatchRepository
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;

		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");

		public BatchRepository(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			return await Batch.CreateAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, totalItemsCount, startingIndex).ConfigureAwait(false);
		}

		public async Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId)
		{
			return await Batch.GetAsync(_serviceFactory, workspaceArtifactId, artifactId).ConfigureAwait(false);
		}

		public async Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return await Batch.GetLastAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId).ConfigureAwait(false);
		}

		public async Task<IEnumerable<int>> GetAllNewBatchesIdsAsync(int workspaceArtifactId)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef()
					{
						Guid = BatchObjectTypeGuid
					},
					Condition = $"'{StatusGuid}' == '{BatchStatus.New}'"
				};

				QueryResult result = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 1, 1).ConfigureAwait(false);
				return result.Objects.Select(x => x.ArtifactID);
			}
		}
	}
}