using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture]
    public class ObjectTypeRepositoryTests
    {
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IObjectTypeManager> _objectTypeManager;
        private Mock<IArtifactGuidManager> _artifactGuidManager;
        private Mock<IRelativityObjectManager> _relativityObjectManager;
        private Mock<IHelper> _helper;

        private ObjectTypeRepository _sut;

        private const string _OBJECT_TYPE_NAME = "Fancy Object";
        private const int _OBJECT_TYPE_ARTIFACT_TYPE_ID = 1111;
        private const int _OBJECT_TYPE_ARTIFACT_ID = 2222;
        private const int _OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID = 3333;
        private const int _WORKSPACE_ID = 9999;

        private readonly Guid _objectTypeGuid = Guid.NewGuid();


        [SetUp]
        public void SetUp()
        {
            var apiLogMock = new Mock<IAPILog>();
            _helper = new Mock<IHelper>();
            _helper
                .Setup(x => x.GetLoggerFactory().GetLogger().ForContext<ObjectTypeRepository>())
                .Returns(apiLogMock.Object);

            _objectTypeManager = new Mock<IObjectTypeManager>();
            _artifactGuidManager = new Mock<IArtifactGuidManager>();
            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System)).Returns(_artifactGuidManager.Object);
            _servicesMgrFake.Setup(x => x.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System)).Returns(_objectTypeManager.Object);
            _relativityObjectManager = new Mock<IRelativityObjectManager>();

            _sut = new ObjectTypeRepository(_WORKSPACE_ID, _servicesMgrFake.Object, _helper.Object, _relativityObjectManager.Object);
        }

        [Test]
        public void CreateObjectType_ShouldCreateObjectType()
        {
            // Arrange
            _objectTypeManager.Setup(x => x.CreateAsync(_WORKSPACE_ID, It.IsAny<ObjectTypeRequest>())).ReturnsAsync(_OBJECT_TYPE_ARTIFACT_ID);

            // Act
            int actualObjectTypeArtifactId = _sut.CreateObjectType(_objectTypeGuid, _OBJECT_TYPE_NAME, _OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID);

            // Assert
            actualObjectTypeArtifactId.Should().Be(_OBJECT_TYPE_ARTIFACT_ID);
        }

        [Test]
        public void CreateObjectType_ShouldThrowException_WhenObjectTypeManagerFails()
        {
            // Arrange
            _objectTypeManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<ObjectTypeRequest>())).Throws<NotFoundException>();

            // Act
            Action action = () => _sut.CreateObjectType(_objectTypeGuid, _OBJECT_TYPE_NAME, _OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID);

            // Assert
            action.ShouldThrow<NotFoundException>();
        }

        [Test]
        public void CreateObjectType_ShouldThrowException_WhenArtifactGuidManagerFails()
        {
            // Arrange
            _artifactGuidManager.Setup(x => 
                    x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<Guid>>()))
                .Throws<InvalidOperationException>();

            // Act
            Action action = () => _sut.CreateObjectType(_objectTypeGuid, _OBJECT_TYPE_NAME, _OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID);

            // Assert
            action.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void RetrieveObjectTypeDescriptorArtifactTypeId_ShouldReturnObjectTypeArtifacTypeId()
        {
            // Arrange
            _artifactGuidManager.Setup(x => x.ReadSingleArtifactIdAsync(_WORKSPACE_ID, _objectTypeGuid)).ReturnsAsync(_OBJECT_TYPE_ARTIFACT_ID);
            _objectTypeManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, _OBJECT_TYPE_ARTIFACT_ID)).ReturnsAsync(new ObjectTypeResponse()
            {
                ArtifactTypeID = _OBJECT_TYPE_ARTIFACT_TYPE_ID
            });

            // Act
            int actualObjectTypeArtifactTypeId = _sut.RetrieveObjectTypeDescriptorArtifactTypeId(_objectTypeGuid);

            // Assert
            actualObjectTypeArtifactTypeId.Should().Be(_OBJECT_TYPE_ARTIFACT_TYPE_ID);
        }

        [Test]
        public void RetrieveObjectTypeDescriptorArtifactTypeId_ShouldThrowException_WhenObjectTypeManagerFails()
        {
            // Arrange
            _objectTypeManager.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>())).Throws<NotFoundException>();

            // Act
            Action action = () => _sut.RetrieveObjectTypeDescriptorArtifactTypeId(_objectTypeGuid);

            // Assert
            action.ShouldThrow<TypeLoadException>()
                .Which.InnerException.Should().BeOfType<NotFoundException>();
        }

        [Test]
        public void RetrieveObjectTypeDescriptorArtifactTypeId_ShouldThrowException_WhenArtifactGuidManagerFails()
        {
            // Arrange
            _artifactGuidManager.Setup(x => x.ReadSingleArtifactIdAsync(_WORKSPACE_ID, _objectTypeGuid)).Throws<NotFoundException>();

            // Act
            Action action = () => _sut.RetrieveObjectTypeDescriptorArtifactTypeId(_objectTypeGuid);

            // Assert
            action.ShouldThrow<TypeLoadException>()
                .Which.InnerException.Should().BeOfType<NotFoundException>();
        }

        [Test]
        public void RetrieveObjectTypeDescriptorArtifactTypeId_ShouldThrow_WhenObjectTypeNotFound()
        {
            // Arrange
            _objectTypeManager.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(null);

            // Act
            Action action = () => _sut.RetrieveObjectTypeDescriptorArtifactTypeId(_objectTypeGuid);

            // Assert
            action.ShouldThrow<TypeLoadException>()
                .Which.InnerException.Should().BeOfType<NotFoundException>();
        }

        [Test]
        public void RetrieveObjectTypeArtifactId_ShouldReturnObjectTypeArtifactId()
        {
            // Arrange
            List<RelativityObject> relativityObjects = new List<RelativityObject>()
            {
                new RelativityObject()
                {
                    ArtifactID = _OBJECT_TYPE_ARTIFACT_ID
                }
            };
            _relativityObjectManager.Setup(x => x.Query(It.Is<QueryRequest>(qr => qr.Condition.Contains(_OBJECT_TYPE_NAME)), It.IsAny<ExecutionIdentity>())).Returns(relativityObjects);

            // Act
            int? actualArtifactId = _sut.RetrieveObjectTypeArtifactId(_OBJECT_TYPE_NAME);

            // Assert
            actualArtifactId.Should().NotBeNull();
            actualArtifactId.Value.Should().Be(_OBJECT_TYPE_ARTIFACT_ID);
        }

        [Test]
        public void RetrieveObjectTypeArtifactId_ShouldReturnNull_WhenObjectTypeDoesntExist()
        {
            // Act
            int? actualArtifactId = _sut.RetrieveObjectTypeArtifactId(_OBJECT_TYPE_NAME);

            // Assert
            actualArtifactId.Should().BeNull();
        }

        [Test]
        public void GetObjectType_ShouldReturnObjectType()
        {
            // Arrange
            List<RelativityObject> relativityObjects = PrepareRelativityObjects();
            _relativityObjectManager.Setup(x => x.Query(It.Is<QueryRequest>(qr => qr.Condition.Contains($"IN [{_OBJECT_TYPE_ARTIFACT_ID}]")), It.IsAny<ExecutionIdentity>()))
                .Returns(relativityObjects);

            // Act
            ObjectTypeDTO actualDto = _sut.GetObjectType(_OBJECT_TYPE_ARTIFACT_ID);

            // Assert
            actualDto.Name.Should().Be(_OBJECT_TYPE_NAME);
            actualDto.ArtifactId.Should().Be(_OBJECT_TYPE_ARTIFACT_ID);
            actualDto.Guids.Single().Should().Be(_objectTypeGuid);
            actualDto.DescriptorArtifactTypeId.Should().Be(_OBJECT_TYPE_ARTIFACT_TYPE_ID);
            actualDto.ParentArtifactId.Should().Be(_OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID);
            actualDto.ParentArtifactTypeId.Should().Be(_OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID);
        }

        [Test]
        public void GetObjectTypeID_ShouldReturnObjectTypeIdForGivenName()
        {
            // Arrange
            List<RelativityObject> relativityObjects = new List<RelativityObject>()
            {
                new RelativityObject()
                {
                    FieldValues = new List<FieldValuePair>()
                    {
                        new FieldValuePair()
                        {
                            Field = new Field()
                            {
                                Name = "DescriptorArtifactTypeID"
                            },
                            Value = _OBJECT_TYPE_ARTIFACT_TYPE_ID
                        }
                    }
                }
            };
            _relativityObjectManager.Setup(x => x.Query(It.Is<QueryRequest>(qr => qr.Condition.Contains(_OBJECT_TYPE_NAME)), It.IsAny<ExecutionIdentity>()))
                .Returns(relativityObjects);
            
            // Act
            int actualObjectTypeId = _sut.GetObjectTypeID(_OBJECT_TYPE_NAME);

            // Assert
            actualObjectTypeId.Should().Be(_OBJECT_TYPE_ARTIFACT_TYPE_ID);
        }

        [Test]
        public void GetRdoGuidToArtifactIdMap_ShouldReturnGuidToArtifactIdDictionary()
        {
            // Arrange
            List<RelativityObject> relativityObjects = PrepareRelativityObjects();
            _relativityObjectManager.Setup(x => x.Query(It.IsAny<QueryRequest>(), It.IsAny<ExecutionIdentity>()))
                .Returns(relativityObjects);

            // Act
            Dictionary<Guid, int> actualDictionary = _sut.GetRdoGuidToArtifactIdMap();

            // Assert
            actualDictionary.Count.Should().Be(1);
            actualDictionary.Keys.Single().Should().Be(_objectTypeGuid);
            actualDictionary.Values.Single().Should().Be(_OBJECT_TYPE_ARTIFACT_TYPE_ID);
        }

        private List<RelativityObject> PrepareRelativityObjects()
        {
            List<RelativityObject> relativityObjects = new List<RelativityObject>()
            {
                new RelativityObject()
                {
                    ArtifactID = _OBJECT_TYPE_ARTIFACT_ID,
                    ParentObject = new RelativityObjectRef()
                    {
                        ArtifactID = _OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID
                    },
                    FieldValues = new List<FieldValuePair>()
                    {
                        new FieldValuePair()
                        {
                            Field = new Field()
                            {
                                Name = "Name"
                            },
                            Value = _OBJECT_TYPE_NAME
                        },
                        new FieldValuePair()
                        {
                            Field = new Field()
                            {
                                Name = "ParentArtifactTypeID"
                            },
                            Value = _OBJECT_TYPE_PARENT_ARTIFACT_TYPE_ID
                        },
                        new FieldValuePair()
                        {
                            Field = new Field()
                            {
                                Name = "DescriptorArtifactTypeID"
                            },
                            Value = _OBJECT_TYPE_ARTIFACT_TYPE_ID
                        }
                    },
                    Guids = new List<Guid>()
                    {
                        _objectTypeGuid
                    }
                }
            };
            return relativityObjects;
        }
    }
}