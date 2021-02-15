using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.System.Core;

namespace Relativity.Sync.Tests.Unit.RDOs
{
    [TestFixture]
    public class RdoManagerTests
    {
        private const int OBJECT_TYPE_ID = 5;
        private const int WORKSPACE_ID = 6;

        private const int CREATED_FIELD_ID = 7;

        private Mock<IObjectManager> _objectManagerMock;
        private Mock<ISyncServiceManager> _syncServicesMgrMock;
        private Mock<IFieldManager> _fieldManagerMock;
        private Mock<IObjectTypeManager> _objectTypeManagerMock;
        private Mock<ISyncLog> _syncLogMock;
        private RdoManager _sut;
        private Mock<IRdoGuidProvider> _rdoGuidProviderMock;
        private Mock<IArtifactGuidManager> _artifactGuidManagerMock;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManager>();
            _syncServicesMgrMock = new Mock<ISyncServiceManager>();
            _fieldManagerMock = new Mock<IFieldManager>();
            _objectTypeManagerMock = new Mock<IObjectTypeManager>();
            _syncLogMock = new Mock<ISyncLog>();
            _rdoGuidProviderMock = new Mock<IRdoGuidProvider>();
            _artifactGuidManagerMock = new Mock<IArtifactGuidManager>();

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System))
                .Returns(_objectManagerMock.Object);

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
                .Returns(_objectTypeManagerMock.Object);

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IFieldManager>(ExecutionIdentity.System)).Returns(
                _fieldManagerMock.Object);

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
                .Returns(_artifactGuidManagerMock.Object);

            _objectManagerMock.Setup(x =>
                    x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject()
                        {
                            ArtifactID = 1, Guids = new List<Guid> {Guid.NewGuid()},
                            FieldValues = new List<FieldValuePair>
                            {
                                new FieldValuePair {Value = 1},
                                new FieldValuePair {Value = 2}
                            }
                        }
                    },
                    ResultCount = 1
                });

            _rdoGuidProviderMock.Setup(x => x.GetValue<SampleRdo>()).Returns(SampleRdo.ExpectedRdoInfo);

            SetupFieldManager(_fieldManagerMock);

            _sut = new RdoManager(_syncLogMock.Object, _syncServicesMgrMock.Object, _rdoGuidProviderMock.Object);
        }

        private void SetupFieldManager(Mock<IFieldManager> fieldManagerMock)
        {
            fieldManagerMock.Setup(x => x.CreateYesNoFieldAsync(WORKSPACE_ID, It.IsAny<YesNoFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock
                .Setup(x => x.CreateWholeNumberFieldAsync(WORKSPACE_ID, It.IsAny<WholeNumberFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock
                .Setup(x => x.CreateFixedLengthFieldAsync(WORKSPACE_ID, It.IsAny<FixedLengthFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock.Setup(x => x.CreateLongTextFieldAsync(WORKSPACE_ID, It.IsAny<LongTextFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock
                .Setup(x => x.CreateSingleObjectFieldAsync(WORKSPACE_ID, It.IsAny<SingleObjectFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
        }

        [Test]
        public async Task EnsureTypeExist_ShouldCreateType_WhenDoesNotExist()
        {
            // Arrange
            const int createdTypeArtifactId = 3;

            _objectManagerMock.Setup(x => x.QueryAsync(WORKSPACE_ID,
                    It.Is<QueryRequest>(q => q.Condition == $"'Guid' == '{SampleRdo.ExpectedRdoInfo.TypeGuid}'"), 0, 1))
                .ReturnsAsync(new QueryResult() {Objects = new List<RelativityObject>()});

            _objectTypeManagerMock
                .Setup(x => x.CreateAsync(WORKSPACE_ID,
                    It.Is<ObjectTypeRequest>(q => q.Name == SampleRdo.ExpectedRdoInfo.Name)))
                .ReturnsAsync(createdTypeArtifactId);


            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WORKSPACE_ID);

            // Assert
            _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(WORKSPACE_ID, createdTypeArtifactId,
                It.Is<List<Guid>>(l => l.Contains(SampleRdo.ExpectedRdoInfo.TypeGuid))));
            
            foreach (RdoFieldInfo fieldInfo in SampleRdo.ExpectedRdoInfo.Fields.Values)
            {
                _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(WORKSPACE_ID, CREATED_FIELD_ID,
                    It.Is<List<Guid>>(l => l.Contains(fieldInfo.Guid))));
            }
        }

        [Test]
        public async Task EnsureTypeExists_ShouldCreateMissingFields()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.QueryAsync(WORKSPACE_ID,
                    It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int) ArtifactType.Field), 0, int.MaxValue))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            Guids = SampleRdo.ExpectedRdoInfo.Fields.Keys.Take(1).ToList()
                        }
                    }
                });

            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WORKSPACE_ID).ConfigureAwait(false);

            // Assert
            var missingFieldInfo = SampleRdo.ExpectedRdoInfo.Fields.Values.Skip(1).First();

            _fieldManagerMock.Verify(x => x.CreateFixedLengthFieldAsync(WORKSPACE_ID, It.Is<FixedLengthFieldRequest>(
                r => r.Name == missingFieldInfo.Name && r.IsRequired == missingFieldInfo.IsRequired &&
                     r.Length == missingFieldInfo.TextLenght)));

            _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(WORKSPACE_ID, CREATED_FIELD_ID,
                It.Is<List<Guid>>(l => l.Contains(missingFieldInfo.Guid))));
        }

        [Test]
        public async Task EnsureTypeExists_ShouldNotCreateAnything_WhenTypeAlreadyExists()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.QueryAsync(WORKSPACE_ID, It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int) ArtifactType.Field), 0, int.MaxValue))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            Guids = SampleRdo.ExpectedRdoInfo.Fields.Keys.ToList()
                        }
                    }
                });
            
            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WORKSPACE_ID);

            // Assert
            _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(WORKSPACE_ID, It.IsAny<int>(),
                It.IsAny<List<Guid>>()), Times.Never);
            
            _objectTypeManagerMock.Verify(x => x.CreateAsync(WORKSPACE_ID, It.IsAny<ObjectTypeRequest>()), Times.Never);
        }
    }
}