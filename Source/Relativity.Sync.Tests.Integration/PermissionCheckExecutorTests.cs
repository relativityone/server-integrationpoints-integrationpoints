using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
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
		private Mock<IObjectManager> _objectManagerFake;
		private Mock<IObjectTypeManager> _objectTypeManagerFake;
		private Mock<ISourceServiceFactoryForUser> _sourceServiceFactoryForUser;
		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactoryForUser;
		private Mock<IDestinationServiceFactoryForAdmin> _destinationServiceFactoryForAdmin;
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
			_configurationStub = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
				DestinationFolderArtifactId = _DATA_DESTINATION_ARTIFACT_ID,
				DestinationWorkspaceArtifactId = _DESTINATION_WORKSPACE_ARTIFACT_ID
			};

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReporting(containerBuilder);

			_objectManagerFake = new Mock<IObjectManager>();
			_objectTypeManagerFake = new Mock<IObjectTypeManager>();
			_sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			_destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			_destinationServiceFactoryForAdmin = new Mock<IDestinationServiceFactoryForAdmin>();
			_permissionManager = new Mock<IPermissionManager>();

			containerBuilder.RegisterInstance(_sourceServiceFactoryForUser.Object)
				.As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(_destinationServiceFactoryForUser.Object)
				.As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterInstance(_destinationServiceFactoryForAdmin.Object)
				.As<IDestinationServiceFactoryForAdmin>();
			containerBuilder.RegisterType<SourcePermissionCheck>().As<IPermissionCheck>();
			containerBuilder.RegisterType<DestinationPermissionCheck>().As<IPermissionCheck>();
			containerBuilder.RegisterType<SyncObjectTypeManager>().As<ISyncObjectTypeManager>();

			_sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IPermissionManager>())
				.ReturnsAsync(_permissionManager.Object);

			_destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IPermissionManager>())
				.ReturnsAsync(_permissionManager.Object);

			_destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectTypeManager>())
				.ReturnsAsync(_objectTypeManagerFake.Object);

			_destinationServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManagerFake.Object);

			const int tagArtifactId = 111;
			_objectManagerFake.Setup(x =>
					x.QueryAsync(It.IsAny<int>(),
						It.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == (int) ArtifactType.ObjectType),
						It.IsAny<int>(), It.IsAny<int>()))
						.ReturnsAsync(new QueryResult()
					{
						Objects = new List<RelativityObject>()
						{
							new RelativityObject()
							{
								ArtifactID = tagArtifactId
							}
						}
					});

			_objectTypeManagerFake.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new ObjectTypeResponse());

			_container = containerBuilder.Build();

			_permissionManager = SetUpPermissionManager();
		}

		[Test]
		public async Task ValidationResultShouldReturnCompleted()
		{
			// Arrange
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

		private Mock<IPermissionManager> SetUpPermissionManager()
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