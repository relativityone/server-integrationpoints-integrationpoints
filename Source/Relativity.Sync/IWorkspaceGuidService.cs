using System;
using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal interface IWorkspaceGuidService
    {
        Task<Guid> GetWorkspaceGuidAsync(int workspaceArtifactId);
    }
}