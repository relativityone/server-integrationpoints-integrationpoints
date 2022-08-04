using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Tab;
using Relativity.Services.Interfaces.Tab.Models;

namespace kCura.IntegrationPoints.Core.Services.Tabs
{
    public class TabService : ITabService
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;

        public TabService(IServicesMgr servicesMgr, IHelper helper)
        {
            _servicesMgr = servicesMgr;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<TabService>();
        }

        public int GetTabId(int workspaceId, int objectTypeId)
        {
            using (ITabManager tabManager = _servicesMgr.CreateProxy<ITabManager>(ExecutionIdentity.CurrentUser))
            {
                List<NavigationTabResponse> allNavigationTabs = tabManager.GetAllNavigationTabs(workspaceId).GetAwaiter().GetResult();
                NavigationTabResponse requestedTab = allNavigationTabs.FirstOrDefault(x => x.ObjectTypeIdentifier?.Value?.ArtifactTypeID == objectTypeId);

                if (requestedTab == null)
                {
                    _logger.LogError("Cannot find tab for Object Type Artifact ID: {objectTypeId} in workspace ID: {workspaceId}", objectTypeId, workspaceId);
                    throw new NotFoundException($"Cannot find tab for Object Type Artifact ID: {objectTypeId} in workspace ID: {workspaceId}");
                }

                return requestedTab.ArtifactID;
            }
        }
    }
}
