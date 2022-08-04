namespace kCura.IntegrationPoints.Core.Tagging
{
    public interface ISourceWorkspaceTagCreator
    {
        int CreateDestinationWorkspaceTag(int destinationWorkspaceId, int jobHistoryInstanceId, int? federatedInstanceId);
    }
}