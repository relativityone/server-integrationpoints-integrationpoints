namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal interface IRetriableStreamBuilderFactory
    {
        IRetriableStreamBuilder Create(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName);
    }
}