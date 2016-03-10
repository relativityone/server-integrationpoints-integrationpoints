using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

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
			IEnumerable<Result<Workspace>> workspaces = query.ExecuteQuery().Results;
			List<WorkspaceModel> result = workspaces.Select(
				workspace => new WorkspaceModel()
				{
					DisplayName = String.Format("{0} [Id:{1}]", workspace.Artifact.Name, workspace.Artifact.ArtifactID),
					Value = workspace.Artifact.ArtifactID
				}).ToList();

			return result;
		}
	}
}