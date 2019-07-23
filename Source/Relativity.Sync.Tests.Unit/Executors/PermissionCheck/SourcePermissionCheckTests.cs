﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PermissionCheck;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors.PermissionCheck
{
	[TestFixture]
	public class SourcePermissionCheckTests
	{
		private SourcePermissionCheck _instance;

		private Mock<ISyncLog> _logger;
		private Mock<ISourceServiceFactoryForUser> _sourceServiceFactory;
		private CancellationToken _cancellationToken;

		private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission

		private readonly Guid IntegrationPoint = new Guid("03d4f67e-22c9-488c-bee6-411f05c52e01");
		private readonly Guid JobHistory = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
		private readonly Guid SourceProvider = new Guid("5be4a1f7-87a8-4cbe-a53f-5027d4f70b80");
		private readonly Guid DestinationProvider = new Guid("d014f00d-f2c0-4e7a-b335-84fcb6eae980");

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;
			_logger = new Mock<ISyncLog>();
			_sourceServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			_instance = new SourcePermissionCheck(_logger.Object,_sourceServiceFactory.Object);
		}

		[Test]
		public async Task ValidateAsyncGoldFlowTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.IsValid.Should().BeTrue();
		}

		[Test]
		public async Task UserShouldNotHavePermissionToViewIpTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(IntegrationPoint)))))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to view Integration Points.");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToAccessThisWorkspace()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(-1, 
				It.IsAny<List<PermissionRef>>(), It.IsAny<int>())).Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to access this workspace.");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToViewTheIpTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(IntegrationPoint))),It.IsAny<int>()))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to view the Integration Point.");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToAddJobHistoryRdoTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(JobHistory)))))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to add Job History RDOs.");
		}

		[Test]
		public async Task UserShouldNotHaveArtifactPermissionToViewSourceProviderRdoTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(SourceProvider)))))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to view Source Provider RDOs.");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToViewDestinationProviderRdoTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(DestinationProvider)))))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to view Destination Provider RDOs.");
		}

		[Test]
		public async Task UserShouldNotHaveInstancePermissionToViewSourceProviderRdoTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(SourceProvider))), It.IsAny<int>()))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to view the Source Provider RDO.");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToExportInTheSourceWorkspaceTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to export in the source workspace.");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToEditDocumentsInThisWorkspaceTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to edit documents in this workspace.");
		}

		[Test]
		public async Task ExecuteAllowExportPermissionTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			var permissionValuesDefault = new List<PermissionValue> { new PermissionValue { Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>())).ReturnsAsync(permissionValuesDefault);
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValuesDefault);

			var permissionToExport = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_EXPORT_PERMISSION_ID, Selected = false } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
				.ReturnsAsync(permissionToExport);

			var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _EDIT_DOCUMENT_PERMISSION_ID, Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
				.ReturnsAsync(permissionToEdit);
			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to export in the source workspace.");
		}

		[Test]
		public async Task ExecuteAllowEditDocumentPermissionTest()
		{
			// Arrange
			const int testWorkspaceArtifactId = 105789;
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(testWorkspaceArtifactId);

			Mock<IPermissionManager> permissionManager = ArrangeTests();

			var permissionValuesDefault = new List<PermissionValue> { new PermissionValue { Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>())).ReturnsAsync(permissionValuesDefault);
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValuesDefault);

			var permissionToExport = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_EXPORT_PERMISSION_ID, Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
				.ReturnsAsync(permissionToExport);

			var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _EDIT_DOCUMENT_PERMISSION_ID, Selected = false } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
				.ReturnsAsync(permissionToEdit);
			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			actualResult.Messages.ElementAt(0).ShortMessage.Should()
				.Be("User does not have permission to edit documents in this workspace.");
		}

		private Mock<IPermissionManager> ArrangeTests()
		{
			var permissionManager = new Mock<IPermissionManager>();
			_sourceServiceFactory.Setup(x => x.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(permissionManager.Object);

			var permissionValuesDefault = new List<PermissionValue> { new PermissionValue { Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>())).ReturnsAsync(permissionValuesDefault);
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValuesDefault);

			var permissionToExport = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_EXPORT_PERMISSION_ID, Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
				.ReturnsAsync(permissionToExport);

			var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _EDIT_DOCUMENT_PERMISSION_ID, Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
				.ReturnsAsync(permissionToEdit);

			return permissionManager;
		}
	}
}