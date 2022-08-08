using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Interfaces.Field.Models;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySourceRdo
{
    [TestFixture, Category("Unit")]
    public class RelativitySourceWorkspaceRdoInitializerTests : TestBase
    {
        private const int _DESTINATION_WORKSPACE_ID = 581555;

        private ISourceWorkspaceRepository _sourceWorkspaceRepository;
        private IRelativitySourceRdoObjectType _relativitySourceRdoObjectType;
        private IRelativitySourceRdoDocumentField _relativitySourceRdoDocumentField;
        private IRelativitySourceRdoFields _relativitySourceRdoFields;

        private RelativitySourceWorkspaceRdoInitializer _instance;

        public override void SetUp()
        {
            _sourceWorkspaceRepository = Substitute.For<ISourceWorkspaceRepository>();
            _relativitySourceRdoObjectType = Substitute.For<IRelativitySourceRdoObjectType>();
            _relativitySourceRdoDocumentField = Substitute.For<IRelativitySourceRdoDocumentField>();
            _relativitySourceRdoFields = Substitute.For<IRelativitySourceRdoFields>();

            IHelper helper = Substitute.For<IHelper>();
            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            IRelativitySourceRdoHelpersFactory helpersFactory = Substitute.For<IRelativitySourceRdoHelpersFactory>();

            repositoryFactory.GetSourceWorkspaceRepository(_DESTINATION_WORKSPACE_ID).Returns(_sourceWorkspaceRepository);

            helpersFactory.CreateRelativitySourceRdoDocumentField(_sourceWorkspaceRepository).Returns(_relativitySourceRdoDocumentField);
            helpersFactory.CreateRelativitySourceRdoFields().Returns(_relativitySourceRdoFields);
            helpersFactory.CreateRelativitySourceRdoObjectType(_sourceWorkspaceRepository).Returns(_relativitySourceRdoObjectType);

            _instance = new RelativitySourceWorkspaceRdoInitializer(helper, repositoryFactory, helpersFactory);
        }

        [Test]
        public void ItShouldInitializeDestinationWorkspace()
        {
            const int expectedSourceJobDescriptorId = 612268;

            _relativitySourceRdoObjectType.CreateObjectType(_DESTINATION_WORKSPACE_ID, SourceWorkspaceDTO.ObjectTypeGuid,
                    IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, (int)ArtifactType.Case)
                .Returns(expectedSourceJobDescriptorId);

            // ACT
            int actualSourceJobDescriptorId = _instance.InitializeWorkspaceWithSourceWorkspaceRdo(_DESTINATION_WORKSPACE_ID);

            // ASSERT
            Assert.That(actualSourceJobDescriptorId, Is.EqualTo(expectedSourceJobDescriptorId));

            _relativitySourceRdoObjectType.Received(1)
                .CreateObjectType(_DESTINATION_WORKSPACE_ID, SourceWorkspaceDTO.ObjectTypeGuid, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, (int)ArtifactType.Case);
            _relativitySourceRdoFields.Received(1).CreateFields(_DESTINATION_WORKSPACE_ID, Arg.Any<IDictionary<Guid, BaseFieldRequest>>());
            _relativitySourceRdoDocumentField.Received(1).CreateDocumentField(_DESTINATION_WORKSPACE_ID, SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid,
                IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, expectedSourceJobDescriptorId);
        }
    }
}