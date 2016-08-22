using System.Collections.Generic;

namespace kCura.IntegrationPoints.Web.SignalRHubs
{
    public class IntegrationPointDataHubInput
    {
        public IntegrationPointDataHubInput()
        { }

        public IntegrationPointDataHubInput(int workspaceId, int artifactId, int userId, string connectionId)
        {
            WorkspaceId = workspaceId;
            ArtifactId = artifactId;
            UserId = userId;
            ConnectionIds = new List<string>() { connectionId };
        }

        public int WorkspaceId { get; set; }
        public int ArtifactId { get; set; }
        public int UserId { get; set; }
        public List<string> ConnectionIds { get; set; }
    }
}