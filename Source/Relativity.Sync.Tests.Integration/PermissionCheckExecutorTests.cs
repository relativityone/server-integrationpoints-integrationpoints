using System.Collections.Generic;
using System.Linq;
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
using Relativity.Sync.Executors.PermissionCheck.DocumentPermissionChecks;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    public class PermissionCheckExecutorTests
    {
        private IContainer _container;
        private Mock<IPermissionManager> _permissionManagerFake;
        private Mock<IObjectManager> _objectManagerFake;
        private Mock<IObjectTypeManager> _objectTypeManagerFake;
        private Mock<ISourceServiceFactoryForUser> _sourceServiceFactoryForUserFake;
        private Mock<ISourceServiceFactoryForAdmin> _sourceServiceFactoryForAdminFake;
        private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactoryForUserFake;
        private Mock<IDestinationServiceFactoryForAdmin> _destinationServiceFactoryForAdminFake;
        private ConfigurationStub _configurationStubFake;

        private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
        private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission
        private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 27564;
        private const int _DATA_DESTINATION_ARTIFACT_ID = 23842;
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 21321;

        [SetUp]
        public void SetUp()
        {
            _configurationStubFake = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
                DestinationFolderArtifactId = _DATA_DESTINATION_ARTIFACT_ID,
                DestinationWorkspaceArtifactId = _DESTINATION_WORKSPACE_ARTIFACT_ID
            };

            ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.MockReportingWithProgress(containerBuilder);

            _objectManagerFake = new Mock<IObjectManager>();
            _objectTypeManagerFake = new Mock<IObjectTypeManager>();
            _sourceServiceFactoryForUserFake = new Mock<ISourceServiceFactoryForUser>();
            _sourceServiceFactoryForAdminFake = new Mock<ISourceServiceFactoryForAdmin>();
            _destinationServiceFactoryForUserFake = new Mock<IDestinationServiceFactoryForUser>();
            _destinationServiceFactoryForAdminFake = new Mock<IDestinationServiceFactoryForAdmin>();
            _permissionManagerFake = new Mock<IPermissionManager>();

            containerBuilder.RegisterInstance(_configurationStubFake).AsImplementedInterfaces();
            containerBuilder.RegisterInstance(_sourceServiceFactoryForUserFake.Object)
                .As<ISourceServiceFactoryForUser>();
            containerBuilder.RegisterInstance(_sourceServiceFactoryForAdminFake.Object)
                .As<ISourceServiceFactoryForAdmin>();
            containerBuilder.RegisterInstance(_destinationServiceFactoryForUserFake.Object)
                .As<IDestinationServiceFactoryForUser>();
            containerBuilder.RegisterInstance(_destinationServiceFactoryForAdminFake.Object)
                .As<IDestinationServiceFactoryForAdmin>();
            containerBuilder.RegisterType<SourceDocumentPermissionCheck>().As<IPermissionCheck>();
            containerBuilder.RegisterType<DestinationDocumentPermissionCheck>().As<IPermissionCheck>();
            containerBuilder.RegisterType<SyncObjectTypeManager>().As<ISyncObjectTypeManager>();

            _sourceServiceFactoryForUserFake.Setup(x => x.CreateProxyAsync<IPermissionManager>())
                .ReturnsAsync(_permissionManagerFake.Object);

            _sourceServiceFactoryForAdminFake.Setup(x => x.CreateProxyAsync<IPermissionManager>())
                .ReturnsAsync(_permissionManagerFake.Object);

            _destinationServiceFactoryForUserFake.Setup(x => x.CreateProxyAsync<IPermissionManager>())
                .ReturnsAsync(_permissionManagerFake.Object);

            _destinationServiceFactoryForAdminFake.Setup(x => x.CreateProxyAsync<IPermissionManager>())
                .ReturnsAsync(_permissionManagerFake.Object);

            _destinationServiceFactoryForUserFake.Setup(x => x.CreateProxyAsync<IObjectTypeManager>())
                .ReturnsAsync(_objectTypeManagerFake.Object);

            _destinationServiceFactoryForAdminFake.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManagerFake.Object);

            _destinationServiceFactoryForAdminFake.Setup(x => x.CreateProxyAsync<IObjectTypeManager>())
                .ReturnsAsync(_objectTypeManagerFake.Object);

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

            _permissionManagerFake = SetUpPermissionManager();
        }

        [Test]
        public async Task Execute_ShouldReturnCompleted()
        {
            // Arrange
            IExecutor<IPermissionsCheckConfiguration> instance = _container.Resolve<IExecutor<IPermissionsCheckConfiguration>>();
            
            // Act
            ExecutionResult validationResult = await instance.ExecuteAsync(_configurationStubFake, CompositeCancellationToken.None).ConfigureAwait(false);

            //Assert
            validationResult.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task Execute_ShouldReturnFailed()
        {
            // Arrange
            var permissionToImport = new List<PermissionValue> { new PermissionValue { Selected = false, PermissionID = _ALLOW_IMPORT_PERMISSION_ID } };
            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
                It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionToImport);

            IExecutor<IPermissionsCheckConfiguration> instance = _container.Resolve<IExecutor<IPermissionsCheckConfiguration>>();

            // Act
            ExecutionResult validationResult = await instance.ExecuteAsync(_configurationStubFake, CompositeCancellationToken.None).ConfigureAwait(false);

            //Assert
            validationResult.Status.Should().Be(ExecutionStatus.Failed);
            validationResult.Message.Should().Be("Permission checks failed. See messages for more details.");
        }

        private Mock<IPermissionManager> SetUpPermissionManager()
        {
            var permissionValue = new List<PermissionValue> { new PermissionValue { Selected = true } };
            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>()))
                .ReturnsAsync(permissionValue);
            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValue);
            var permissionToExport = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_EXPORT_PERMISSION_ID, Selected = true } };
            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
                .ReturnsAsync(permissionToExport);

            var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _EDIT_DOCUMENT_PERMISSION_ID, Selected = true } };
            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
                .ReturnsAsync(permissionToEdit);
            var permissionToImport = new List<PermissionValue> { new PermissionValue { Selected = true, PermissionID = _ALLOW_IMPORT_PERMISSION_ID } };
            _permissionManagerFake.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
                It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionToImport);

            return _permissionManagerFake;
        }
    }
}