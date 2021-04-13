using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
	public class SourceProviderHelper : WorkspaceHelperBase
	{
		public SourceProviderHelper(WorkspaceTest workspace) : base(workspace)
		{
		}

		public void CreateLDAP(WorkspaceTest workspace)
		{
			var sourceProvider = new SourceProviderTest
			{
				WorkspaceId = workspace.ArtifactId,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.LDAP_NAME,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.LDAP,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		public void CreateRelativity(WorkspaceTest workspace)
		{
			var sourceProvider = new SourceProviderTest
			{
				WorkspaceId = workspace.ArtifactId,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY_NAME,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		public void CreateFTP(WorkspaceTest workspace)
		{
			var sourceProvider = new SourceProviderTest
			{
				WorkspaceId = workspace.ArtifactId,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.FTP_NAME,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.FTP,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		public void CreateLoadFile(WorkspaceTest workspace)
		{
			var sourceProvider = new SourceProviderTest
			{
				WorkspaceId = workspace.ArtifactId,
				Name = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE_NAME,
				Identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE,
				ApplicationIdentifier = Const.INTEGRATION_POINTS_APP_GUID
			};

			Workspace.SourceProviders.Add(sourceProvider);
		}

		
	}
}