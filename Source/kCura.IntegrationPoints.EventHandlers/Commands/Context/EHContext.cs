using kCura.EventHandler;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Context
{
    public class EHContext : IEHContext
    {
        public IEHHelper Helper { get; set; }

        public Artifact ActiveArtifact { get; set; }

        public string TempTableNameWithParentArtifactsToDelete { get; set; }

    }
}
