using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.Field.Models;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySourceRdo
{
    [TestFixture, Category("Unit")]
    public class RelativitySourceJobRdoInitializerTests : TestBase
    {
        private const int _DESTINATION_WORKSPACE_ID = 416177;
        private const int _SOURCE_WORKSPACE_ARTIFACT_TYPE_ID = 778398;

        private ISourceJobRepository _sourceJobRepository;
        private IRelativitySourceRdoObjectType _relativitySourceRdoObjectType;
        private IRelativitySourceRdoDocumentField _relativitySourceRdoDocumentField;
        private IRelativitySourceRdoFields _relativitySourceRdoFields;

        private RelativitySourceJobRdoInitializer _instance;

        public override void SetUp()
        {
            _sourceJobRepository = Substitute.For<ISourceJobRepository>();
            _relativitySourceRdoObjectType = Substitute.For<IRelativitySourceRdoObjectType>();
            _relativitySourceRdoDocumentField = Substitute.For<IRelativitySourceRdoDocumentField>();
            _relativitySourceRdoFields = Substitute.For<IRelativitySourceRdoFields>();

            IHelper helper = Substitute.For<IHelper>();
            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            IRelativitySourceRdoHelpersFactory helpersFactory = Substitute.For<IRelativitySourceRdoHelpersFactory>();

            repositoryFactory.GetSourceJobRepository(_DESTINATION_WORKSPACE_ID).Returns(_sourceJobRepository);

            helpersFactory.CreateRelativitySourceRdoDocumentField(_sourceJobRepository).Returns(_relativitySourceRdoDocumentField);
            helpersFactory.CreateRelativitySourceRdoFields().Returns(_relativitySourceRdoFields);
            helpersFactory.CreateRelativitySourceRdoObjectType(_sourceJobRepository).Returns(_relativitySourceRdoObjectType);

            _instance = new RelativitySourceJobRdoInitializer(helper, repositoryFactory, helpersFactory);
        }

        [Test]
        public void ItShouldInitializeDestinationWorkspace()
        {
            const int expectedSourceJobDescriptorId = 888672;

            _relativitySourceRdoObjectType.CreateObjectType(_DESTINATION_WORKSPACE_ID, SourceJobDTO.ObjectTypeGuid, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
                    _SOURCE_WORKSPACE_ARTIFACT_TYPE_ID)
                .Returns(expectedSourceJobDescriptorId);

            // ACT
            int actualSourceJobDescriptorId = _instance.InitializeWorkspaceWithSourceJobRdo(_DESTINATION_WORKSPACE_ID, _SOURCE_WORKSPACE_ARTIFACT_TYPE_ID);

            // ASSERT
            Assert.That(actualSourceJobDescriptorId, Is.EqualTo(expectedSourceJobDescriptorId));

            _relativitySourceRdoObjectType.Received(1)
                .CreateObjectType(_DESTINATION_WORKSPACE_ID, SourceJobDTO.ObjectTypeGuid, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, _SOURCE_WORKSPACE_ARTIFACT_TYPE_ID);
            _relativitySourceRdoFields.Received(1).CreateFields(_DESTINATION_WORKSPACE_ID, Arg.Any<IDictionary<Guid, BaseFieldRequest>>());
            _relativitySourceRdoDocumentField.Received(1)
                .CreateDocumentField(_DESTINATION_WORKSPACE_ID, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
                    expectedSourceJobDescriptorId);
        }
    }
}