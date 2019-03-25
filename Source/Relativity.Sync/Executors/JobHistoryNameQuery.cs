using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class JobHistoryNameQuery : IJobHistoryNameQuery
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly ISyncLog _logger;

		public JobHistoryNameQuery(ISourceServiceFactoryForUser sourceServiceFactoryForUser, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_logger = logger;
		}

		public async Task<string> GetJobNameAsync(int jobHistoryArtifactId, int sourceWorkspaceArtifactId, CancellationToken token)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"'ArtifactID' == {jobHistoryArtifactId}",
					ObjectType = new ObjectTypeRef() {Guid = new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9")},
					IncludeNameInQueryResult = true
					//Fields = new List<FieldRef>
					//{
					//	new FieldRef {Name = "Name"}
					//}
				};
				const int start = 0;
				const int length = 1;
				QueryResult queryResult;

				try
				{
					queryResult = await objectManager.QueryAsync(sourceWorkspaceArtifactId, request, start, length, token, new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to query for Job History with Artifact ID: {jobHistoryArtifactId}", jobHistoryArtifactId);
					throw;
				}

				if (!queryResult.Objects.Any())
				{
					_logger.LogError("Couldn't find Job History with Artifact ID: {jobHistoryArtifactId}", jobHistoryArtifactId);
					throw new SyncException($"Couldn't find Job History with Artifact ID: {jobHistoryArtifactId}");
				}

				return queryResult.Objects.First().Name;
			}
		}
	}
}