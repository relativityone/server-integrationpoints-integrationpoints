using System;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.RDO;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerWorkspaceRepository : IWorkspaceRepository
	{
		private readonly IObjectQueryManagerAdaptor _objectQueryManagerAdaptor;

		public KeplerWorkspaceRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor)
		{
			_objectQueryManagerAdaptor = objectQueryManagerAdaptor;
			_objectQueryManagerAdaptor.ArtifactTypeId = 8;
		}

		public WorkspaceDTO Retrieve(int workspaceArtifactId)
		{
			ObjectQueryResultSet resultSet = null;
			try
			{
				var query = new Query()
				{
					Fields = new [] { "Name" },
					Condition = $"'ArtifactID' == {workspaceArtifactId}",
					TruncateTextFields = false 
				};

				resultSet = _objectQueryManagerAdaptor.RetrieveAsync(query, String.Empty).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve Workspace", e);
			}

			RdoHelper.CheckObjectQueryResultSet(resultSet);
			QueryDataItemResult result = resultSet.Data.DataResults.FirstOrDefault();

			if (result == null)
			{
				throw new Exception($"Unable to retrieve workspace: No workspace for given artifact id {workspaceArtifactId}");	
			}

			var workspaceDto = new WorkspaceDTO()
			{
				ArtifactId = result.ArtifactId,
				Name = result.Fields[0].Value as string
			};

			return workspaceDto;
		}
	}
}