namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    public interface ICorrespondingJobDelete
    {
        void DeleteCorrespondingJob(int workspaceId, int integrationPointArtifactId);
    }
}
