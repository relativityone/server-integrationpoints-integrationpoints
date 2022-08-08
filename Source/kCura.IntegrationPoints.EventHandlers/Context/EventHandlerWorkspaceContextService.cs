using kCura.IntegrationPoints.Common.Context;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Context
{
    public class EventHandlerWorkspaceContextService : IWorkspaceContext
    {
        private readonly IEHHelper _helper;

        public EventHandlerWorkspaceContextService(IEHHelper helper)
        {
            _helper = helper;
        }

        public int GetWorkspaceID() => _helper.GetActiveCaseID();
    }
}
