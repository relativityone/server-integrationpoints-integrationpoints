using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.TagsCreation
{
	internal sealed class WorkspaceNameQuery : IWorkspaceNameQuery
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IAPILog _logger;

		public WorkspaceNameQuery(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IAPILog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_logger = logger;
		}

		public async Task<string> GetWorkspaceNameAsync(int workspaceArtifactId)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"'ArtifactID' == {workspaceArtifactId}",
					ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
					Fields = new List<FieldRef>() { new FieldRef() { Name = "Name" } }
				};
				const int workspaceId = -1;
				const int start = 0;
				const int length = 1;
				QueryResult result = await objectManager.QueryAsync(workspaceId, request, start, length).ConfigureAwait(false);

				if (!result.Objects.Any())
				{
					_logger.LogError("Couldn't find workspace Artifact ID: {workspaceArtifactId}", workspaceArtifactId);
					throw new SyncException($"Couldn't find workspace Artifact ID: {workspaceArtifactId}");
				}

				return result.Objects.First().Name;
			}
		}
	}
}