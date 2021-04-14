using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public interface IRelativityHelpers
    {
        WorkspaceHelper WorkspaceHelper { get; }
        AgentHelper AgentHelper { get; }
        JobHelper JobHelper { get; }
    }
}