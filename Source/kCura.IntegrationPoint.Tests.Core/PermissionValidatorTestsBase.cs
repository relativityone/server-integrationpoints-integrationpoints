using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
    internal static class ValidationResultExtensions
    {
        internal static void Check(this ValidationResult result, bool expected)
        {
            Assert.That(result.IsValid, Is.EqualTo(expected));

            if (expected)
            {
                Assert.That(result.MessageTexts.Count, Is.Zero);
            }
            else
            {
                Assert.That(result.MessageTexts.Count, Is.Positive);
            }
        }
    }

    public class PermissionValidatorTestsBase : TestBase
    {
        protected const int INTEGRATION_POINT_ID = 101323;
        protected const int _SOURCE_WORKSPACE_ID = 100532;
        protected const int _SOURCE_PROVIDER_ID = 39309;
        protected const int _SAVED_SEARCH_ID = 9492;
        protected const int _ARTIFACT_TYPE_ID = 1232;
        protected const int _DESTINATION_WORKSPACE_ID = 349234;
        protected const int _DESTINATION_ARTIFACT_TYPE_ID = 1233;
        protected const int _DESTINATION_FOLDER_ID = 986454;
        protected const int _DESTINATION_PROVIDER_ID = 42042;

        protected Guid _SOURCE_PROVIDER_GUID = ObjectTypeGuids.SourceProviderGuid;
        protected Guid _DESTINATION_PROVIDER_GUID = ObjectTypeGuids.DestinationProviderGuid;

        protected IRepositoryFactory _repositoryFactory;
        protected IPermissionRepository _sourcePermissionRepository;
        protected IPermissionRepository _destinationPermissionRepository;
        protected ISerializer _serializer;
        protected IServiceContextHelper ServiceContextHelper;
        protected IObjectTypeRepository _objectTypeRepository;
        protected IntegrationPointProviderValidationModel _validationModel;

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _sourcePermissionRepository = Substitute.For<IPermissionRepository>();
            _destinationPermissionRepository = Substitute.For<IPermissionRepository>();
            _objectTypeRepository = Substitute.For<IObjectTypeRepository>();
            ServiceContextHelper = Substitute.For<IServiceContextHelper>();
            ServiceContextHelper.WorkspaceID.Returns(_SOURCE_WORKSPACE_ID);
            _repositoryFactory.GetPermissionRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_sourcePermissionRepository);
            _repositoryFactory.GetPermissionRepository(Arg.Is(_DESTINATION_WORKSPACE_ID)).Returns(_destinationPermissionRepository);
            _repositoryFactory.GetObjectTypeRepository(Arg.Is(_SOURCE_WORKSPACE_ID)).Returns(_objectTypeRepository);
            _serializer = Substitute.For<ISerializer>();

            _validationModel = new IntegrationPointProviderValidationModel()
            {
                ArtifactId = INTEGRATION_POINT_ID,
                SourceConfiguration =
                    $"{{ \"SavedSearchArtifactId\":{_SAVED_SEARCH_ID}, \"SourceWorkspaceArtifactId\":{_SOURCE_WORKSPACE_ID}, \"TargetWorkspaceArtifactId\":{_DESTINATION_WORKSPACE_ID} }}",
                DestinationConfiguration = new ImportSettings { ArtifactTypeId = _ARTIFACT_TYPE_ID },
                SourceProviderArtifactId = _SOURCE_PROVIDER_ID,
                DestinationProviderArtifactId = _DESTINATION_PROVIDER_ID,
                ObjectTypeGuid = Guid.Empty
            };

            _validationModel.DestinationConfiguration = new ImportSettings
            {
                ArtifactTypeId = _ARTIFACT_TYPE_ID,
                DestinationArtifactTypeId = _DESTINATION_ARTIFACT_TYPE_ID,
                DestinationFolderArtifactId = _DESTINATION_FOLDER_ID,
                MoveExistingDocuments = false,
                UseFolderPathInformation = true
            };

            _serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
                .Returns(new SourceConfiguration
                {
                    SavedSearchArtifactId = _SAVED_SEARCH_ID,
                    SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
                    TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
                });
        }
    }
}
