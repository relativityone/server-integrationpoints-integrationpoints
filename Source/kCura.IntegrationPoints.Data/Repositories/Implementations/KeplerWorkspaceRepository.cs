using System;
using System.Linq;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerWorkspaceRepository : KeplerServiceBase, IWorkspaceRepository
	{
		public KeplerWorkspaceRepository(IObjectQueryManagerAdaptor objectQueryManagerAdaptor) : base(objectQueryManagerAdaptor)
		{
			this.ObjectQueryManagerAdaptor.ArtifactTypeId = (int) ArtifactType.Case;
		}

		public WorkspaceDTO Retrieve(int workspaceArtifactId)
		{
			ArtifactDTO[] workspaces = null;
			var query = new Query()
			{
				Fields = new[] { "Name" },
				Condition = $"'ArtifactID' == {workspaceArtifactId}",
				TruncateTextFields = false
			};

			try
			{
				 workspaces = this.RetrieveAllArtifactsAsync(query).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve Workspace", e);
			}

			ArtifactDTO workspace = workspaces.FirstOrDefault();

			if (workspace == null || (workspace.Fields[0].Value as string) == null)
			{
				throw new Exception("Unable to retrieve Workspace");	
			}

			var workspaceDto = new WorkspaceDTO()
			{
				ArtifactId = workspace.ArtifactId,
				Name = (string) workspace.Fields[0].Value
			};

			return workspaceDto;
		}
	}
}