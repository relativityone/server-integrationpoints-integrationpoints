using kCura.EventHandler;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Context
{
    public interface IEHContext
    {
        IEHHelper Helper { get; }
        Artifact ActiveArtifact { get; }
        string TempTableNameWithParentArtifactsToDelete { get; set; }
    }
}
