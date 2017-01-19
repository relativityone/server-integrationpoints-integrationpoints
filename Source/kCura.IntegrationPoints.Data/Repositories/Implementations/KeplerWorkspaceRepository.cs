using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Data.Extensions;
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
				 workspaces = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve workspace", e);
			}

			return Convert(workspaces).FirstOrDefault();
		}

		public IEnumerable<WorkspaceDTO> RetrieveAll()
		{
			var query = new global::Relativity.Services.ObjectQuery.Query()
			{
				Fields = new[] { "Name" },
			};

			ArtifactDTO[] artifactDtos = null;
			try
			{
				artifactDtos = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to retrieve workspaces", e);
			}

			return Convert(artifactDtos);
		}

		private IEnumerable<WorkspaceDTO> Convert(IEnumerable<ArtifactDTO> artifactDtos)
		{
			var workspaces = new List<WorkspaceDTO>();

			foreach (ArtifactDTO artifactDto in artifactDtos)
			{
				workspaces.Add(new WorkspaceDTO()
				{
					ArtifactId = artifactDto.ArtifactId,
					Name = (string)artifactDto.Fields[0].Value
				});
			}

			return workspaces;
		}

	}
}