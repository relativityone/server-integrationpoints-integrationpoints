using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public class PermissionManagerStub : KeplerStubBase<IPermissionManager>
	{
		public void SetupPermissionsCheck(bool workspaceOrArtifactInstancePermissionsValue = true, bool artifactTypePermissionsValue = true)
		{
			Mock.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>()))
				.Returns((int workspaceId, List<PermissionRef> permissions) =>
				{
					return Task.FromResult(permissions.Select(p => new PermissionValue
					{
						PermissionID = p.PermissionID,
						Selected = artifactTypePermissionsValue
					}).ToList());
				});

			Mock.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>()))
				.Returns((int workspaceId, List<PermissionRef> permissions, int artifactId) =>
				{
					return Task.FromResult(new List<PermissionValue>
					{
						new PermissionValue
						{
							PermissionID = artifactId,
							Selected = workspaceOrArtifactInstancePermissionsValue
						}
					});
				});
		}
	}
}
