using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySourceRdo
{
    [TestFixture, Category("Unit")]
    public class RelativitySourceRdoFieldsTests : TestBase
    {
        private const int _WORKSPACE_ID = 216578;
        private IFieldQueryRepository _fieldQueryRepository;
        private IArtifactGuidRepository _artifactGuidRepository;
        private IFieldRepository _fieldRepository;
        private RelativitySourceRdoFields _instance;

        public override void SetUp()
        {
            _fieldQueryRepository = Substitute.For<IFieldQueryRepository>();
            _artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
            _fieldRepository = Substitute.For<IFieldRepository>();

            IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
            repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepository);
            repositoryFactory.GetFieldQueryRepository(_WORKSPACE_ID).Returns(_fieldQueryRepository);
            repositoryFactory.GetFieldRepository(_WORKSPACE_ID).Returns(_fieldRepository);

            _instance = new RelativitySourceRdoFields(repositoryFactory);
        }

        [Test]
        public void ItShouldCreateNonExistingFields()
        {
            Guid fieldGuid = Guid.NewGuid();
            string fieldName = "field_name_316";
            const int descriptorArtifactTypeID = 100138;

            const int fieldId = 444186;

            IDictionary<Guid, BaseFieldRequest> fields = new Dictionary<Guid, BaseFieldRequest>
            {
                {
                    fieldGuid, new WholeNumberFieldRequest
                    {
                        Name = fieldName,
                        ObjectType = new ObjectTypeIdentifier
                        {
                            ArtifactTypeID = descriptorArtifactTypeID
                        }
                    }
                }
            };

            _artifactGuidRepository.GuidExists(fieldGuid).Returns(false);
            _fieldQueryRepository.RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(),
                    Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)))
                .Returns((ArtifactDTO)null);
            _fieldRepository.CreateObjectTypeField(fields[fieldGuid]).Returns(fieldId);

            // ACT
            _instance.CreateFields(_WORKSPACE_ID, fields);

            // ASSERT
            _artifactGuidRepository.Received(1).GuidExists(fieldGuid);
            _fieldQueryRepository.Received(1)
                .RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(), Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)));
            _fieldRepository.Received(1).CreateObjectTypeField(fields[fieldGuid]);
            _artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(fieldId, fieldGuid);
        }

        [Test]
        public void ItShouldUpdateExistingFields()
        {
            Guid fieldGuid = Guid.NewGuid();
            string fieldName = "field_name_526";
            const int descriptorArtifactTypeID = 488469;

            const int fieldId = 431240;

            IDictionary<Guid, BaseFieldRequest> fields = new Dictionary<Guid, BaseFieldRequest>
            {
                {
                    fieldGuid, new WholeNumberFieldRequest
                    {
                        Name = fieldName,
                        ObjectType = new ObjectTypeIdentifier
                        {
                            ArtifactTypeID = descriptorArtifactTypeID
                        }
                    }
                }
            };

            _artifactGuidRepository.GuidExists(fieldGuid).Returns(false);
            _fieldQueryRepository.RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(),
                    Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)))
                .Returns(new ArtifactDTO(fieldId, 744, "", new List<ArtifactFieldDTO>()));

            // ACT
            _instance.CreateFields(_WORKSPACE_ID, fields);

            // ASSERT
            _artifactGuidRepository.Received(1).GuidExists(fieldGuid);
            _fieldQueryRepository.Received(2)
                .RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(), Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)));
            _artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(fieldId, fieldGuid);

            _fieldRepository.DidNotReceive().CreateObjectTypeField(fields[fieldGuid]);
        }

        [Test]
        public void ItShouldSkipCreationForExistingField()
        {
            Guid fieldGuid = Guid.NewGuid();

            IDictionary<Guid, BaseFieldRequest> fields = new Dictionary<Guid, BaseFieldRequest>
            {
                {
                    fieldGuid, new WholeNumberFieldRequest
                    {
                        Name = "field_name_246",
                        ObjectType = new ObjectTypeIdentifier
                        {
                            ArtifactTypeID = 402331
                        }
                    }
                }
            };

            _artifactGuidRepository.GuidExists(fieldGuid).Returns(true);

            // ACT
            _instance.CreateFields(_WORKSPACE_ID, fields);

            // ASSERT
            _artifactGuidRepository.Received(1).GuidExists(fieldGuid);

            _fieldQueryRepository.DidNotReceive().RetrieveField(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<HashSet<string>>());
            _fieldRepository.DidNotReceive().CreateObjectTypeField(Arg.Any<BaseFieldRequest>());
            _artifactGuidRepository.DidNotReceive().InsertArtifactGuidForArtifactId(Arg.Any<int>(), Arg.Any<Guid>());
        }
    }
}
