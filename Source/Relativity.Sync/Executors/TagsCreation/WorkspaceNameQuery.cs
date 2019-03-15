using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.TagsCreation
{
	internal sealed class WorkspaceNameQuery : IWorkspaceNameQuery
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;

		public WorkspaceNameQuery(ISourceServiceFactoryForUser sourceServiceFactoryForUser)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
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
				QueryResult result = await objectManager.QueryAsync(-1, request, 0, 1).ConfigureAwait(false);

				if (!result.Objects.Any())
				{
					throw new SyncException($"Query for ArtifactID = {workspaceArtifactId} did not return any results.");
				}

				return result.Objects.First().Name;
			}
		}
	}
}