using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Web.Models
{
	public class WorkspaceModel
	{
		private WorkspaceModel()
		{
		}

		public int Value { get; private set; }
		public String DisplayName { get; private set; }

		public static List<WorkspaceModel> GetWorkspaceModels(IRSAPIClient context)
		{
			GetWorkspacesQuery query = new GetWorkspacesQuery(context);
			List<Artifact> artifacts = query.ExecuteQuery().QueryArtifacts;
			List<WorkspaceModel> result = new List<WorkspaceModel>(artifacts.Count);

			foreach (var artifact in artifacts)
			{
				result.Add(new WorkspaceModel() { DisplayName = String.Format("{0} [Id:{1}]",artifact.Name, artifact.ArtifactID), Value = artifact.ArtifactID} );
			}
			return result;
		}
	}
}