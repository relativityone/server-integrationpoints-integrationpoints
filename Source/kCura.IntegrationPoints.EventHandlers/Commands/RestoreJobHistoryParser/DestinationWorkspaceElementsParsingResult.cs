namespace kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser
{
    public class DestinationWorkspaceElementsParsingResult
    {
        public static DestinationWorkspaceElementsParsingResult Empty { get; } = new DestinationWorkspaceElementsParsingResult(null, null);
    
        public string InstanceName { get; set; }

        public string WorkspaceName { get; set; }

        public DestinationWorkspaceElementsParsingResult(string workspacePart, string instancePart)
        {
            InstanceName = instancePart;
            WorkspaceName = workspacePart;
        }

        public DestinationWorkspaceElementsParsingResult(string workspaceName, string instanceName, int? instanceId)
        {
            InstanceName = instanceId.HasValue ? $"{instanceName} - {instanceId}" : instanceName;
            WorkspaceName = workspaceName;
        }
    }
}