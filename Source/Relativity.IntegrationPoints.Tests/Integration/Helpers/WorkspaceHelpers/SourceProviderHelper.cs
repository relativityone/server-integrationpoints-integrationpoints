using Relativity.IntegrationPoints.Tests.Integration.Models;

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
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		public void CreateRelativity()
		{
			var sourceProvider = new SourceProviderTest
			{
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY_NAME,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		public void CreateFTP()
		{
			var sourceProvider = new SourceProviderTest
			{
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.FTP_NAME,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.FTP,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		public void CreateLoadFile()
		{
			var sourceProvider = new SourceProviderTest
			{
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE_NAME,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		public SourceProviderTest CreateMyFirstProvider()
		{
			var myFirstProvider = new SourceProviderTest
			{
				Name = "My First Provider",
				Identifier = Const.MY_FIRST_PROVIDER_GUID,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID,
			};
			
			Workspace.SourceProviders.Add(myFirstProvider);
			return myFirstProvider;
		}
	}
}