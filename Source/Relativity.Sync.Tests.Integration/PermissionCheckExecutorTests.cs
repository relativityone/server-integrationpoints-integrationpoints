using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PermissionCheck;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class PermissionCheckExecutorTests
	{
		private IContainer _container;
		private Mock<IPermissionManager> _permissionManager;
		private Mock<ISourceServiceFactoryForUser> _sourceServiceFactory;
		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactory;
		private ConfigurationStub _configurationStub;

		private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission
		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 27564;
		private const int _DATA_DESTINATION_ARTIFACT_ID = 23842;
		private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 21321;
		[SetUp]
		public void SetUp()
		{
			_sourceServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			_destinationServiceFactory = new Mock<IDestinationServiceFactoryForUser>();
			_permissionManager = new Mock<IPermissionManager>();
			_container = ContainerHelper.CreateContainer(cb =>
			{
				cb.RegisterInstance(_sourceServiceFactory.Object).As<ISourceServiceFactoryForUser>();
				cb.RegisterInstance(_destinationServiceFactory.Object).As<IDestinationServiceFactoryForUser>();
				cb.RegisterType<SourcePermissionCheck>().As<IPermissionCheck>();
				cb.RegisterType<DestinationPermissionCheck>().As<IPermissionCheck>();
			});

			_sourceServiceFactory.Setup(x => x.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(_permissionManager.Object);
			_destinationServiceFactory.Setup(x => x.CreateProxyAsync<IPermissionManager>())
				.ReturnsAsync(_permissionManager.Object);

			_configurationStub = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
				DestinationFolderArtifactId = _DATA_DESTINATION_ARTIFACT_ID,
				DestinationWorkspaceArtifactId = _DESTINATION_WORKSPACE_ARTIFACT_ID
			};
		}

		[Test]
		public async Task ValidationResultShouldReturnCompleted()
		{
			// Arrange
			_permissionManager = ArrangeSet();

			IExecutor<IPermissionsCheckConfiguration> instance = _container.Resolve<IExecutor<IPermissionsCheckConfiguration>>();
			
			// Act
			ExecutionResult validationResult = await instance.ExecuteAsync(_configurationStub, CancellationToken.None).ConfigureAwait(false);

			//Assert
			validationResult.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ValidationResultShouldReturnFailed()
		{
			// Arrange
			_permissionManager = ArrangeSet();

			var permissionToImport = new List<PermissionValue> { new PermissionValue { Selected = false, PermissionID = _ALLOW_IMPORT_PERMISSION_ID } };
			_permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionToImport);

			IExecutor<IPermissionsCheckConfiguration> instance = _container.Resolve<IExecutor<IPermissionsCheckConfiguration>>();

			// Act
			ExecutionResult validationResult = await instance.ExecuteAsync(_configurationStub, CancellationToken.None).ConfigureAwait(false);

			//Assert
			validationResult.Status.Should().Be(ExecutionStatus.Failed);
			validationResult.Message.Should().Be("Permission checks failed. See messages for more details.");
		}

		private Mock<IPermissionManager> ArrangeSet()
		{
			var permissionValue = new List<PermissionValue> { new PermissionValue { Selected = true } };
			_permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>()))
				.ReturnsAsync(permissionValue);
			_permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValue);
			var permissionToExport = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_EXPORT_PERMISSION_ID, Selected = true } };
			_permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
				.ReturnsAsync(permissionToExport);

			var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _EDIT_DOCUMENT_PERMISSION_ID, Selected = true } };
			_permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
				.ReturnsAsync(permissionToEdit);
			var permissionToImport = new List<PermissionValue> { new PermissionValue { Selected = true, PermissionID = _ALLOW_IMPORT_PERMISSION_ID } };
			_permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionToImport);

			return _permissionManager;
		}
	}
}