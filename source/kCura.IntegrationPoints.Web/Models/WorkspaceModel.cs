using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Models
{
    public class WorkspaceModel
    {
        private WorkspaceModel()
        {
        }

        public int Value { get; private set; }
        public String DisplayName { get; private set; }

        public static List<WorkspaceModel> GetWorkspaceModels(IRSAPIClient context, IHtmlSanitizerManager htmlSanitizerManager)
        {
            GetWorkspacesQuery query = new GetWorkspacesQuery(context);
            QueryResultSet<Workspace> resultSet = query.ExecuteQuery();
	        if (!resultSet.Success)
	        {
		        throw new Exception(resultSet.Message);
	        }

            IEnumerable<Result<Workspace>> workspaces = resultSet.Results;
            List<WorkspaceModel> result = workspaces.Select(
                workspace => new WorkspaceModel()
                {
                    DisplayName = Utils.GetFormatForWorkspaceOrJobDisplay(htmlSanitizerManager.Sanitize(workspace.Artifact.Name).CleanHTML, workspace.Artifact.ArtifactID),
                    Value = workspace.Artifact.ArtifactID
                }).ToList();

            return result;
        }
    }
}