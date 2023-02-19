using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;
using ArtifactType = Relativity.ArtifactType;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class PermissionRepositoryTests : TestBase
    {
        private IPermissionRepository _sut;
        private IHelper _helper;
        private IServicesMgr _servicesMgr;
        private IPermissionManager _permissionManager;

        private const int _WORKSPACE_ID = 392834;

        [SetUp]
        public override void SetUp()
        {
            _helper = NSubstitute.Substitute.For<IHelper>();
            _servicesMgr = NSubstitute.Substitute.For<IServicesMgr>();
            _permissionManager = NSubstitute.Substitute.For<IPermissionManager>();

            _helper.GetServicesManager().Returns(_servicesMgr);
            _servicesMgr.CreateProxy<IPermissionManager>(Arg.Is(ExecutionIdentity.CurrentUser)).Returns(_permissionManager);

            _sut = new PermissionRepository(_helper, _WORKSPACE_ID);
        }

        [Test]
        public void UserHasPermissionToAccessWorkspace_UserHasPermissions_ReturnsTrue()
        {
            // Arrange
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier((int) ArtifactType.Case),
                PermissionType = PermissionType.View
            };

            var permissionValues = new List<PermissionValue>()
            {
                new PermissionValue()
                {
                    Selected = true
                }
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID))
                .Returns(permissionValues);

            // Act
            bool result = _sut.UserHasPermissionToAccessWorkspace();

            // Assert
            Assert.IsTrue(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID));
        }

        [Test]
        public void UserHasPermissionToAccessWorkspace_PermissionRequestExcepts_ReturnsFalse()
        {
            // Arrange
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Case),
                PermissionType = PermissionType.View
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID))
                .Throws(new Exception("LEEEDDLE LEEEDDLE LEEELE"));

            // Act
            bool result = _sut.UserHasPermissionToAccessWorkspace();

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID));
        }

        [Test]
        public void UserHasPermissionToAccessWorkspace_InvalidPermissions_ReturnsFalse()
        {
            // Arrange
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Case),
                PermissionType = PermissionType.View
            };

            var permissionValues = new List<PermissionValue>()
            {
                new PermissionValue()
                {
                    Selected = false
                }
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID))
                .Returns(permissionValues);

            // Act
            bool result = _sut.UserHasPermissionToAccessWorkspace();

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID));
        }

        [Test]
        public void UserHasPermissionToAccessWorkspace_NoPermissionsReturned_ReturnsFalse()
        {
            // Arrange
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Case),
                PermissionType = PermissionType.View
            };

            var permissionValues = new List<PermissionValue>()
            {
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID))
                .Returns(permissionValues);

            // Act
            bool result = _sut.UserHasPermissionToAccessWorkspace();

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(-1),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(_WORKSPACE_ID));
        }

        [Test]
        public void UserHasArtifactInstancePermission_UserHasPermissions_ReturnsTrue()
        {
            // Arrange
            const int artifactId = 84903;
            Guid artifactGuid = Guid.NewGuid();
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier(artifactGuid),
                PermissionType = PermissionType.View
            };

            var permissionValues = new List<PermissionValue>()
            {
                new PermissionValue()
                {
                    Selected = true
                }
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(artifactId))
                .Returns(permissionValues);

            // Act
            bool result = _sut.UserHasArtifactInstancePermission(artifactGuid, artifactId, ArtifactPermission.View);

            // Assert
            Assert.IsTrue(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(artifactId));
        }

        [Test]
        public void UserHasArtifactInstancePermission_PermissionRequestExcepts_ReturnsFalse()
        {
            // Arrange
            const int artifactId = 84903;
            Guid artifactGuid = Guid.NewGuid();
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier(artifactGuid),
                PermissionType = PermissionType.View
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(artifactId))
                .Throws(new Exception("SQUAREPANTS"));

            // Act
            bool result = _sut.UserHasArtifactInstancePermission(artifactGuid, artifactId, ArtifactPermission.View);

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType),
                Arg.Is(artifactId));
        }

        [Test]
        public void UserHasArtifactTypePermissions_UserHasPermissions_ReturnsTrue()
        {
            // Arrange
            const int artifactTypeId = 49203;
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier(artifactTypeId),
                PermissionType = PermissionType.View
            };

            var permissionValues = new List<PermissionValue>()
            {
                new PermissionValue()
                {
                    Selected = true
                }
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType))
                .Returns(permissionValues);

            // Act
            bool result = _sut.UserHasArtifactTypePermissions(artifactTypeId, new[] { ArtifactPermission.View });

            // Assert
            Assert.IsTrue(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType));
        }

        [Test]
        public void UserHasArtifactTypePermissions_PermissionRequestExcepts_ReturnsFalse()
        {
            // Arrange
            const int artifactTypeId = 49203;
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier(artifactTypeId),
                PermissionType = PermissionType.View
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType))
                .Throws(new Exception("Patrick"));

            // Act
            bool result = _sut.UserHasArtifactTypePermissions(artifactTypeId, new[] { ArtifactPermission.View });

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.ID == expectedPermissionRef.ArtifactType.ID
                         && x.First().PermissionType == expectedPermissionRef.PermissionType));
        }

        [Test]
        public void UserHasArtifactTypePermissions_MultiplePermissions_PermissionRequestExcepts_ReturnsFalse()
        {
            // Arrange
            const int artifactTypeId = 49203;

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 3
                         && x[0].PermissionType == PermissionType.View
                         && x[1].PermissionType == PermissionType.Edit
                         && x[2].PermissionType == PermissionType.Add))
                .Throws(new Exception("Patrick"));

            // Act
            bool result = _sut.UserHasArtifactTypePermissions(artifactTypeId, new[] { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create });

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 3
                         && x[0].PermissionType == PermissionType.View
                         && x[1].PermissionType == PermissionType.Edit
                         && x[2].PermissionType == PermissionType.Add));
        }

        [Test]
        public void UserHasArtifactTypePermission_UserHasPermissions_ReturnsTrue()
        {
            // Arrange
            Guid artifactGuid = Guid.NewGuid();
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier(artifactGuid),
                PermissionType = PermissionType.View
            };

            var permissionValues = new List<PermissionValue>()
            {
                new PermissionValue()
                {
                    Selected = true
                }
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType))
                .Returns(permissionValues);

            // Act
            bool result = _sut.UserHasArtifactTypePermission(artifactGuid, ArtifactPermission.View );

            // Assert
            Assert.IsTrue(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType));
        }

        [Test]
        public void UserHasArtifactTypePermission_PermissionRequestExcepts_ReturnsFalse()
        {
            // Arrange
            Guid artifactGuid = Guid.NewGuid();
            var expectedPermissionRef = new PermissionRef()
            {
                ArtifactType = new ArtifactTypeIdentifier(artifactGuid),
                PermissionType = PermissionType.View
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType))
                .Throws(new Exception("Squidward"));

            // Act
            bool result = _sut.UserHasArtifactTypePermission(artifactGuid, ArtifactPermission.View);

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().ArtifactType.Guids.SequenceEqual(expectedPermissionRef.ArtifactType.Guids)
                         && x.First().PermissionType == expectedPermissionRef.PermissionType));
        }

        [Test]
        public void UserCanImport_HasPermissions_ReturnsTrue()
        {
            // Arrange
            int permissionId = 158;
            var permissionValue = new PermissionValue()
            {
                PermissionID = permissionId,
                Selected = true
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId))
                .Returns(new List<PermissionValue>() { permissionValue });

            // Act
            bool result = _sut.UserCanImport();

            // Assert
            Assert.IsTrue(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId));
        }

        [Test]
        public void UserCanImport_InvalidPermissions_ReturnsFalse()
        {
            // Arrange
            int permissionId = 158;

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId))
                .Throws(new Exception("Pearl"));

            // Act
            bool result = _sut.UserCanImport();

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId));
        }

        [Test]
        public void UserCanExport_HasPermissions_ReturnsTrue()
        {
            // Arrange
            int permissionId = 159;
            var permissionValue = new PermissionValue()
            {
                PermissionID = permissionId,
                Selected = true
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId))
                .Returns(new List<PermissionValue>() { permissionValue });

            // Act
            bool result = _sut.UserCanExport();

            // Assert
            Assert.IsTrue(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId));
        }

        [Test]
        public void UserCanExport_InvalidPermissions_ReturnsFalse()
        {
            // Arrange
            int permissionId = 159;

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId))
                .Throws(new Exception("Mr. Krabs"));

            // Act
            bool result = _sut.UserCanExport();

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId));
        }

        [Test]
        public void UserCanEditDocuments_HasPermissions_ReturnsTrue()
        {
            // Arrange
            int permissionId = 45;
            var permissionValue = new PermissionValue()
            {
                PermissionID = permissionId,
                Selected = true
            };

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId))
                .Returns(new List<PermissionValue>() { permissionValue });

            // Act
            bool result = _sut.UserCanEditDocuments();

            // Assert
            Assert.IsTrue(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId));
        }

        [Test]
        public void UserCanEditDocuments_InvalidPermissions_ReturnsFalse()
        {
            // Arrange
            int permissionId = 45;

            _permissionManager.GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId))
                .Throws(new Exception("Mr. Krabs"));

            // Act
            bool result = _sut.UserCanEditDocuments();

            // Assert
            Assert.IsFalse(result);
            _permissionManager.Received(1).GetPermissionSelectedAsync(
                Arg.Is(_WORKSPACE_ID),
                Arg.Is<List<PermissionRef>>(
                    x => x.Count() == 1
                         && x.First().PermissionID == permissionId));
        }
    }
}
