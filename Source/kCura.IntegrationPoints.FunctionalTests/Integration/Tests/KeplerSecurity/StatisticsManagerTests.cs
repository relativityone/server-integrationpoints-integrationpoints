using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Utils;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class StatisticsManagerTests : KeplerSecurityTestBase
    {
        private IStatisticsManager _sut;

        [SetUp]
        public void Setup() => _sut = new StatisticsManager(Logger, PermissionRepositoryFactory, Container);

        [TestCaseSource(nameof(Actions))]
        public void Method_ShouldNotThrow_WhenAllPermissionsAreGranted(Func<IStatisticsManager, Task> action)
        {
            ShouldPassWithAllPermissions<PermissionsForStatisticsManager>(() => action(_sut));
        }

        [TestCaseSource(nameof(Actions))]
        public void Method_ShouldThrowInsufficientPermissions(Func<IStatisticsManager, Task> action)
        {
            ShouldThrowInsufficientPermissions(new PermissionSetup[0], () => action(_sut));
        }

        public static IEnumerable<Func<IStatisticsManager ,Task>> Actions()
        {
            yield return (manager => manager.GetDocumentsTotalForFolderAsync(WorkspaceId, 1, 1, true));
            yield return (manager => manager.GetImagesTotalForFolderAsync(WorkspaceId, 1, 1, true));
            yield return (manager => manager.GetNativesTotalForFolderAsync(WorkspaceId, 1, 1, true));
            yield return (manager => manager.GetImagesFileSizeForFolderAsync(WorkspaceId, 1, 1, true));
            yield return (manager => manager.GetNativesFileSizeForFolderAsync(WorkspaceId, 1, 1, true));

            yield return (manager => manager.GetDocumentsTotalForProductionAsync(WorkspaceId, 1));
            yield return (manager => manager.GetImagesTotalForProductionAsync(WorkspaceId, 1));
            yield return (manager => manager.GetNativesTotalForProductionAsync(WorkspaceId, 1));
            yield return (manager => manager.GetNativesTotalForProductionAsync(WorkspaceId, 1));
            yield return (manager => manager.GetImagesFileSizeForProductionAsync(WorkspaceId, 1));
            yield return (manager => manager.GetNativesFileSizeForProductionAsync(WorkspaceId, 1));
        }

        #region Permissions

        class PermissionsForStatisticsManager : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId)
            };
        }

        #endregion
    }
}
