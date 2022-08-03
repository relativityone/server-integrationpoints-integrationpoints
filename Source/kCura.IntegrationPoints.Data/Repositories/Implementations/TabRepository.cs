using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Interfaces.Tab;
using Relativity.Services.Interfaces.Tab.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class TabRepository : ITabRepository
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly int _workspaceArtifactId;

        public TabRepository(IServicesMgr servicesMgr, int workspaceArtifactId)
        {
            _servicesMgr = servicesMgr;
            _workspaceArtifactId = workspaceArtifactId;
        }

        public int? RetrieveTabArtifactId(int objectTypeArtifactId, string tabName)
        {
            using (ITabManager tabManager = _servicesMgr.CreateProxy<ITabManager>(ExecutionIdentity.CurrentUser))
            {
                List<NavigationTabResponse> allNavigationTabs = tabManager.GetAllNavigationTabs(_workspaceArtifactId).GetAwaiter().GetResult();
                NavigationTabResponse requestedTab = allNavigationTabs?.FirstOrDefault(x =>
                    x.ObjectTypeIdentifier?.Value?.ArtifactTypeID == objectTypeArtifactId && x.Name == tabName);

                return requestedTab?.ArtifactID;
            }
        }

        public void Delete(int tabArtifactId)
        {
            using (ITabManager tabManager = _servicesMgr.CreateProxy<ITabManager>(ExecutionIdentity.CurrentUser))
            {
                tabManager.DeleteAsync(_workspaceArtifactId, tabArtifactId).GetAwaiter().GetResult();
            }
        }
    }
}
