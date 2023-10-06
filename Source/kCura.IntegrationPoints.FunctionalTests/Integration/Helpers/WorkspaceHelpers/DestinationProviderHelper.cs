using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class DestinationProviderHelper : WorkspaceHelperBase
    {
        public DestinationProviderHelper(WorkspaceFake workspace) : base(workspace)
        {
        }

        public DestinationProviderFake CreateRelativityProvider()
        {
            var provider =  new DestinationProviderFake()
            {
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY,
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY_NAME
            };

            Workspace.DestinationProviders.Add(provider);

            return provider;
        }

        public DestinationProviderFake CreateLoadFile()
        {
            var destinationProvider = new DestinationProviderFake()
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
