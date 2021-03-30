using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public class PermissionManagerStub : KeplerStubBase<IPermissionManager>
	{
		private readonly FakeUser _user;

		public PermissionManagerStub(FakeUser user)
		{
			_user = user;
		}

		public void SetupPermissionsCheck()
		{
			Mock.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>()))
				.Returns((int workspaceId, List<PermissionRef> permissions) =>
				{
					if (_user.IsAdmin)
					{
						return Task.FromResult(permissions.Select(p => new PermissionValue
						{
							PermissionID = p.PermissionID,
							Selected = true
						}).ToList());
					}

					return null;
				});

			Mock.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>()))
				.Returns((int workspaceId, List<PermissionRef> permissions, int artifactId) =>
				{
					List<PermissionValue> validPermission = new List<PermissionValue>
					{
						new PermissionValue
						{
							PermissionID = artifactId,
							Selected = true
						}
					};

					if (_user.IsAdmin)
					{
						return Task.FromResult(validPermission);
					}

					return Task.FromResult(
						_user.AssignedWorkspaces.Exists(w => w.ArtifactId == artifactId)
							? validPermission
							: null);
				});
		}
	}
}
