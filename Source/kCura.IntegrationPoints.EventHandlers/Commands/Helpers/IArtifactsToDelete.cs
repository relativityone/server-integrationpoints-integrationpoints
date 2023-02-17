using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Helpers
{
    public interface IArtifactsToDelete
    {
        List<int> GetIds();
    }
}
