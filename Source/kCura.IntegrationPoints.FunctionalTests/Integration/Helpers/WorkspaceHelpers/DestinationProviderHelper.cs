using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class DestinationProviderHelper : WorkspaceHelperBase
    {
        public DestinationProviderHelper(WorkspaceTest workspace) : base(workspace)
        {
        }
        
        public DestinationProviderTest CreateRelativityProvider()
        {
            var provider =  new DestinationProviderTest()
            {
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY,
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY_NAME
            };
            
            Workspace.DestinationProviders.Add(provider);

            return provider;
        }

        public DestinationProviderTest CreateLoadFile()
        {
            var destinationProvider = new DestinationProviderTest()
            {
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.LOADFILE,
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.LOADFILE_NAME
            };

            Workspace.DestinationProviders.Add(destinationProvider);

            return destinationProvider;
        }
    }
}