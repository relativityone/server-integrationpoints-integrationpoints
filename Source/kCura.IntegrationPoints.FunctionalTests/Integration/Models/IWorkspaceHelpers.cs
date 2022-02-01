using Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public interface IWorkspaceHelpers
    {
        DestinationProviderHelper DestinationProviderHelper { get; }
        SourceProviderHelper SourceProviderHelper { get; }
        IntegrationPointHelper IntegrationPointHelper { get; }
        IntegrationPointProfileHelper IntegrationPointProfileHelper { get; }
        IntegrationPointTypeHelper IntegrationPointTypeHelper { get; }
        JobHistoryHelper JobHistoryHelper { get; }
        FieldsMappingHelper FieldsMappingHelper { get; }
        DocumentHelper DocumentHelper { get; }
    }
}