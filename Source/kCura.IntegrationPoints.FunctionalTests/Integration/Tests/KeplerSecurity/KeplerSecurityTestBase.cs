using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Logging;
using Relativity.Services;
using Relativity.Services.Permission;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    public abstract class KeplerSecurityTestBase : TestsBase
    {
        protected static int WorkspaceId { get; } = 123;
        protected ILog Logger { get; private set; }

        protected PermissionManagerStub PermissionManager { get; private set; }

        protected IPermissionRepositoryFactory PermissionRepositoryFactory { get; private set; }

        [SetUp]
        public void BaseSetup()
        {
            Logger = Container.Resolve<ILog>();
            PermissionManager = Proxy.PermissionManager;
            PermissionRepositoryFactory = Container.Resolve<IPermissionRepositoryFactory>();

            PermissionManager.GrantNotConfiguredPermissions = false;
        }

        protected void ShouldPassWithAllPermissions<TPermissions>(Func<Task> action)  where TPermissions : PermissionPermutator, new()
        {
            // Arrange
            SetupPermissions(new TPermissions().AllPermissionsGranted());

           // Assert
            action.ShouldNotThrow<InsufficientPermissionException>();
        }

        protected void ShouldThrowInsufficientPermissions(PermissionSetup[] permissions, Func<Task> action)
        {
            // Arrange
            SetupPermissions(permissions);

            // Assert
            action.ShouldThrow<InsufficientPermissionException>();
        }

        protected void SetupPermissions(IEnumerable<PermissionSetup> permissions)
        {
            foreach (var permission in permissions)
            {
                PermissionManager.SetupPermission(permission);
            }
        }

        public static PermissionSetup GetPermissionRefForWorkspace(int workspaceId)
        {
            return new PermissionSetup
            {
                Permission = new PermissionRef()
                {
                    ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Case),
                    PermissionType = PermissionType.View,
                    Name = "Workspace"
                },
                Workspace = -1,
                Instance = workspaceId,
                Granted = true
            };
        }
    }

    public struct PermissionSetup
    {
        public PermissionRef Permission { get; set; }

        public int Workspace { get; set; }

        public int? Instance { get; set; }

        public bool Granted { get; set; }

        public override string ToString()
        {
            string instanceIndicator = Instance.HasValue && Workspace != -1 ? "(for instance)" : "" ;
            return $"[{Permission.Name}: {Permission.PermissionType.Name}, {instanceIndicator} {Granted}";
        }
    }
}
