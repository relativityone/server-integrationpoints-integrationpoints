using Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public interface IRelativityHelpers
    {
        WorkspaceHelper WorkspaceHelper { get; }
        AgentHelper AgentHelper { get; }
        JobHelper JobHelper { get; }
    }
}
