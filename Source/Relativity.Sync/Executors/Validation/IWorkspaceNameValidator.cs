using System.Threading;

namespace Relativity.Sync.Executors.Validation
{
    internal interface IWorkspaceNameValidator
    {
        bool Validate(string workspaceName, int workspaceArtifactId, CancellationToken token);
    }
}