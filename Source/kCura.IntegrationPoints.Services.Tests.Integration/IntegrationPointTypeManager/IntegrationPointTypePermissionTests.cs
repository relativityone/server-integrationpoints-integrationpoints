using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointTypeManager
{
	public class IntegrationPointTypePermissionTests : KeplerServicePermissionsTestsBase
	{
		[IdentifiedTest("b24092e3-255a-4dea-bb9f-610721eb65f3")]
		public void MissingWorkspacePermission()
		{
			var client = Helper.CreateProxy<IIntegrationPointTypeManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointTypes(WorkspaceArtifactId).Result);
		}

		[IdentifiedTest("1ceaec25-751b-4ea6-a795-0390d745a14d")]
		public void MissingJobHistoryViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointType);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateProxy<IIntegrationPointTypeManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointTypes(WorkspaceArtifactId).Result);
		}
	}
}