using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using Relativity.IntegrationPoints.Tests.Common;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class SourceProviderHelper : WorkspaceHelperBase
    {
        public SourceProviderHelper(WorkspaceTest workspace) : base(workspace)
        {
        }

        public void CreateLDAP()
        {
            var sourceProvider = new SourceProviderTest
            {
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.LDAP_NAME,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.LDAP,
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID
            };

            Workspace.SourceProviders.Add(sourceProvider);
        }

        public void CreateRelativity()
        {
            var sourceProvider = new SourceProviderTest
            {
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY_NAME,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY,
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID
            };

            Workspace.SourceProviders.Add(sourceProvider);
        }

        public void CreateFTP()
        {
            var sourceProvider = new SourceProviderTest
            {
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.FTP_NAME,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.FTP,
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID
            };

            Workspace.SourceProviders.Add(sourceProvider);
        }

        public void CreateLoadFile()
        {
            var sourceProvider = new SourceProviderTest
            {
                Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE_NAME,
                Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE,
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID
            };

            Workspace.SourceProviders.Add(sourceProvider);
        }

        public SourceProviderTest CreateMyFirstProvider()
        {
            var myFirstProvider = new SourceProviderTest
            {
                Name = "My First Provider",
                Identifier = MyFirstProvider.Provider.GlobalConstants.FIRST_PROVIDER_GUID,
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
            };
            
            Workspace.SourceProviders.Add(myFirstProvider);
            return myFirstProvider;
        }

        public SourceProviderTest CreateCustomProvider(string name, Guid identifier)
        {
            var customProvider = new SourceProviderTest
            {
                Name = name,
                Identifier = identifier.ToString(),
                ApplicationIdentifier = GlobalConst.INTEGRATION_POINTS_APPLICATION_GUID,
            };

            Workspace.SourceProviders.Add(customProvider);
            return customProvider;
        }
    }
}