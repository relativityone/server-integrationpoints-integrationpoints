using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Tab;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;
using Relativity.Sync.Tests.Common.RDOs;

namespace Relativity.Sync.Tests.Unit.RDOs
{
    [TestFixture]
    public class RdoManagerTests
    {
        private const int WorkspaceId = 6;
        private const int CreatedFieldId = 7;

        private Mock<IObjectManager> _objectManagerMock;
        private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdminMock;
        private Mock<IFieldManager> _fieldManagerMock;
        private Mock<IObjectTypeManager> _objectTypeManagerMock;
        private Mock<ITabManager> _tabManagerMock;
        private Mock<IAPILog> _syncLogMock;
        private RdoManager _sut;
        private Mock<IRdoGuidProvider> _rdoGuidProviderMock;
        private Mock<IArtifactGuidManager> _artifactGuidManagerMock;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManager>();
            _serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
            _fieldManagerMock = new Mock<IFieldManager>();
            _objectTypeManagerMock = new Mock<IObjectTypeManager>();
            _tabManagerMock = new Mock<ITabManager>();
            _syncLogMock = new Mock<IAPILog>();
            _rdoGuidProviderMock = new Mock<IRdoGuidProvider>();
            _artifactGuidManagerMock = new Mock<IArtifactGuidManager>();

            _ = _serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .Returns(Task.FromResult(_objectManagerMock.Object));

            _ = _serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IObjectTypeManager>())
                .Returns(Task.FromResult(_objectTypeManagerMock.Object));

            _ = _serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<ITabManager>())
                .Returns(Task.FromResult(_tabManagerMock.Object));

            _ = _serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IFieldManager>()).Returns(
                Task.FromResult(_fieldManagerMock.Object));

            _ = _serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IArtifactGuidManager>())
                .Returns(Task.FromResult(_artifactGuidManagerMock.Object));

            _ = _objectManagerMock.Setup(x =>
                    x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            ArtifactID = 1, Guids = new List<Guid> { Guid.NewGuid() },
                            FieldValues = new List<FieldValuePair>
                            {
                                new FieldValuePair { Value = 1 },
                                new FieldValuePair { Value = 2 }
                            }
                        }
                    },
                    ResultCount = 1
                });

            _ = _rdoGuidProviderMock.Setup(x => x.GetValue<SampleRdo>()).Returns(SampleRdo.ExpectedRdoInfo);
            _ = _rdoGuidProviderMock.Setup(x => x.GetValue<ExtendedSampleRdo>()).Returns(ExtendedSampleRdo.ExpectedRdoInfo);
            _ = _rdoGuidProviderMock.Setup(x => x.GetValue<RdoWithParent>()).Returns(new RdoTypeInfo
            {
                Fields = new ReadOnlyDictionary<Guid, RdoFieldInfo>(new Dictionary<Guid, RdoFieldInfo>()),
                Name = nameof(RdoWithParent),
                TypeGuid = new Guid("37C93366-5D07-42D0-8EA8-2400B4323F36"),
                ParentTypeGuid = new Guid("22C0EE5E-4CA6-49D3-86F8-63DF2382083D")
            });

            SetupFieldManager(_fieldManagerMock);

            _sut = new RdoManager(_syncLogMock.Object, _serviceFactoryForAdminMock.Object, _rdoGuidProviderMock.Object);
        }

        [Test]
        public async Task EnsureTypeExist_ShouldCreateType_WhenDoesNotExist()
        {
            // Arrange
            const int createdTypeArtifactId = 3;

            _ = _objectManagerMock.Setup(x => x.QueryAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.Condition == $"'Name' == '{SampleRdo.ExpectedRdoInfo.Name}'"),
                    0,
                    1))
                .ReturnsAsync(new QueryResult { Objects = new List<RelativityObject>() });

            _ = _objectTypeManagerMock
                .Setup(x => x.CreateAsync(
                    WorkspaceId,
                    It.Is<ObjectTypeRequest>(q => q.Name == SampleRdo.ExpectedRdoInfo.Name)))
                .ReturnsAsync(createdTypeArtifactId);

            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WorkspaceId);

            // Assert
            _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(
                WorkspaceId,
                createdTypeArtifactId,
                It.Is<List<Guid>>(l => l.Contains(SampleRdo.ExpectedRdoInfo.TypeGuid))));

            foreach (RdoFieldInfo fieldInfo in SampleRdo.ExpectedRdoInfo.Fields.Values)
            {
                _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(
                    WorkspaceId,
                    CreatedFieldId,
                    It.Is<List<Guid>>(l => l.Contains(fieldInfo.Guid))));
            }
        }

        [Test]
        public async Task EnsureTypeExist_ShouldDeleteTabAfterObjectTypeCreation()
        {
            // Arrange
            const int tabArtifactId = 4;
            _ = _objectManagerMock.Setup(x =>
                    x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>(),
                    TotalCount = 0
                });

            _ = _objectManagerMock.Setup(x => x.QueryAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int)ArtifactType.Tab && q.Condition == $"'Object Type' == '{SampleRdo.ExpectedRdoInfo.Name}'"),
                    0,
                    1))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            ArtifactID = tabArtifactId
                        }
                    }
                });

            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WorkspaceId);

            // Assert
            _tabManagerMock.Verify(x => x.DeleteAsync(WorkspaceId, tabArtifactId), Times.Once);
        }

        [Test]
        public async Task EnsureTypeExist_ShouldNotDeleteTab_WhenTabDoesntExist()
        {
            // Arrange
            const int tabArtifactId = 4;
            _ = _objectManagerMock.Setup(x =>
                    x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>(),
                    TotalCount = 0
                });

            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WorkspaceId);

            // Assert
            _tabManagerMock.Verify(x => x.DeleteAsync(WorkspaceId, tabArtifactId), Times.Never);
        }

        [Test]
        public async Task EnsureTypeExists_ShouldRespectObjectGuid()
        {
            // Arrange
            _ = _objectManagerMock.Setup(x => x.QueryAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.Condition == $"'Name' == '{nameof(RdoWithParent)}'"),
                    0,
                    1))
                .ReturnsAsync(new QueryResult { Objects = new List<RelativityObject>() });

            // Act
            await _sut.EnsureTypeExistsAsync<RdoWithParent>(WorkspaceId).ConfigureAwait(false);

            // Assert
            Guid expectedParentGuid = new Guid("22C0EE5E-4CA6-49D3-86F8-63DF2382083D");
            _objectTypeManagerMock.Verify(x => x.CreateAsync(WorkspaceId, It.Is<ObjectTypeRequest>(r => r.ParentObjectType.Value.Guids.Contains(expectedParentGuid))));
        }

        [Test]
        public async Task EnsureTypeExists_ShouldCreateMissingFields()
        {
            // Arrange
            _ = _objectManagerMock.Setup(x => x.QueryAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int)ArtifactType.Field),
                    0,
                    int.MaxValue))
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
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WorkspaceId).ConfigureAwait(false);

            // Assert
            var missingFieldInfo = SampleRdo.ExpectedRdoInfo.Fields.Values.Skip(1).First();

            _fieldManagerMock.Verify(x => x.CreateFixedLengthFieldAsync(
                WorkspaceId,
                It.Is<FixedLengthFieldRequest>(
                r => r.Name == missingFieldInfo.Name && r.IsRequired == missingFieldInfo.IsRequired &&
                     r.Length == missingFieldInfo.TextLength)));

            _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(
                WorkspaceId,
                CreatedFieldId,
                It.Is<List<Guid>>(l => l.Contains(missingFieldInfo.Guid))));
        }

        [Test]
        public async Task EnsureTypeExists_ShouldUpdateFieldWithoutGuidButAddedToFieldsTable()
        {
            // Arrange
            _ = _objectManagerMock.Setup(x => x.QueryAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int)ArtifactType.Field),
                    0,
                    int.MaxValue))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            FieldValues = new List<FieldValuePair>
                            {
                                new FieldValuePair
                                {
                                    Value = nameof(SampleRdo.SomeField)
                                }
                            },
                            Guids = SampleRdo.ExpectedRdoInfo.Fields.Where(x => x.Value.Name != nameof(SampleRdo.SomeField)).Select(x => x.Key).ToList()
                        }
                    }
                });

            // Act
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WorkspaceId).ConfigureAwait(false);

            // Assert
            Guid missingFieldGuid = SampleRdo
                .ExpectedRdoInfo
                .Fields
                .Single(x => x.Value.Name == nameof(SampleRdo.SomeField))
                .Key;
            _fieldManagerMock.Verify(
                x => x.CreateFixedLengthFieldAsync(
                WorkspaceId,
                It.IsAny<FixedLengthFieldRequest>()),
                Times.Never);

            _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(
                WorkspaceId,
                0,
                It.Is<List<Guid>>(l => l.Contains(missingFieldGuid))));
        }

        [Test]
        public async Task EnsureTypeExists_ShouldNotCreateAnything_WhenTypeAlreadyExists()
        {
            // Arrange
            _ = _objectManagerMock.Setup(x => x.QueryAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int)ArtifactType.Field),
                    0,
                    int.MaxValue))
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
            await _sut.EnsureTypeExistsAsync<SampleRdo>(WorkspaceId);

            // Assert
            _artifactGuidManagerMock.Verify(
                x => x.CreateSingleAsync(
                WorkspaceId,
                It.IsAny<int>(),
                It.IsAny<List<Guid>>()),
                Times.Never);

            _objectTypeManagerMock.Verify(x => x.CreateAsync(WorkspaceId, It.IsAny<ObjectTypeRequest>()), Times.Never);
        }

        [Test]
        public void EnsureTypeExists_ShouldThrowDuplicateNameException_WhenTypeHaveTwoFieldsWithTheSameNames()
        {
            // Arrange
            const string duplicatedFieldName = "duplicated Field Name";

            _ = _objectManagerMock.Setup(x => x.QueryAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.ObjectType.ArtifactTypeID == (int)ArtifactType.Field),
                    0,
                    int.MaxValue))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            Guids = SampleRdo.ExpectedRdoInfo.Fields.Keys.ToList(),
                            FieldValues = new List<FieldValuePair>
                            {
                                new FieldValuePair
                                {
                                    Field = new Field(),
                                    Value = duplicatedFieldName
                                }
                            }
                        },
                        new RelativityObject
                        {
                            Guids = SampleRdo.ExpectedRdoInfo.Fields.Keys.ToList(),
                            FieldValues = new List<FieldValuePair>
                            {
                                new FieldValuePair
                                {
                                    Field = new Field(),
                                    Value = duplicatedFieldName
                                }
                            }
                        }
                    }
                });

            // Act
            Func<Task> function = async () => await _sut.EnsureTypeExistsAsync<SampleRdo>(WorkspaceId).ConfigureAwait(false);

            // Assert
            _ = function.Should().Throw<DuplicateNameException>();
            _artifactGuidManagerMock.Verify(
                x => x.CreateSingleAsync(
                    WorkspaceId,
                    It.IsAny<int>(),
                    It.IsAny<List<Guid>>()),
                Times.Never);

            _objectTypeManagerMock.Verify(x => x.CreateAsync(WorkspaceId, It.IsAny<ObjectTypeRequest>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_ShouldCreateObject()
        {
            // Arrange
            const int createdArtifactId = 5;
            _ = _objectManagerMock.Setup(x => x.CreateAsync(WorkspaceId, It.IsAny<CreateRequest>()))
                .ReturnsAsync(new CreateResult { Object = new RelativityObject { ArtifactID = createdArtifactId } });

            SampleRdo o = new SampleRdo();

            // Act
            await _sut.CreateAsync(WorkspaceId, o).ConfigureAwait(false);

            // Assert
            _ = o.ArtifactId.Should().Be(createdArtifactId);
        }

        [Test]
        public async Task GetAsync_ShouldRetrieveAllFields()
        {
            // Arrange
            const int objectArtifactId = 70;
            SampleRdo sourceRdo = new SampleRdo
            {
                ArtifactId = objectArtifactId,
                SomeField = 4,
                OptionalTextField = "Hakuna matata"
            };

            SetupObjectManagerToReturnRdo(sourceRdo);

            // Act
            SampleRdo result = await _sut.GetAsync<SampleRdo>(WorkspaceId, sourceRdo.ArtifactId).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(sourceRdo);
            VerifyFieldWasQueried(nameof(SampleRdo.SomeField), Times.Once());
            VerifyFieldWasQueried(nameof(SampleRdo.OptionalTextField), Times.Once());
        }

        [Test]
        public async Task GetAsync_ShouldRetrieveOnlySelectedFields()
        {
            // Arrange
            const int objectArtifactId = 70;
            SampleRdo sourceRdo = new SampleRdo
            {
                ArtifactId = objectArtifactId,
                SomeField = 4,
                OptionalTextField = "Hakuna matata"
            };

            SetupObjectManagerToReturnRdo(sourceRdo);

            // Act
            SampleRdo result = await _sut.GetAsync<SampleRdo>(WorkspaceId, sourceRdo.ArtifactId, x => x.SomeField)
                .ConfigureAwait(false);

            // Assert
            _ = result.SomeField.Should().Be(sourceRdo.SomeField);
            _ = result.OptionalTextField.Should().BeNullOrEmpty();
            VerifyFieldWasQueried(nameof(SampleRdo.SomeField), Times.Once());
            VerifyFieldWasQueried(nameof(SampleRdo.OptionalTextField), Times.Never());
        }

        [Test]
        public async Task SetValuesAsync_ShouldUpdateAllValues()
        {
            // Arrange
            SampleRdo rdo = new SampleRdo
            {
                ArtifactId = 70,
                SomeField = 3,
                OptionalTextField = "Lorem ipsum"
            };

            // Act
            await _sut.SetValuesAsync(WorkspaceId, rdo).ConfigureAwait(false);

            // Assert
            VerifyFieldWasUpdated(rdo, nameof(SampleRdo.SomeField), rdo.SomeField);
            VerifyFieldWasUpdated(rdo, nameof(SampleRdo.OptionalTextField), rdo.OptionalTextField);
        }

        [Test]
        public async Task SetValuesAsync_ShouldUpdateOnlySelectedValues()
        {
            // Arrange
            SampleRdo rdo = new SampleRdo
            {
                ArtifactId = 70,
                SomeField = 3,
                OptionalTextField = "Lorem ipsum"
            };

            // Act
            const int newValue = 5;
            await _sut.SetValueAsync(WorkspaceId, rdo, x => x.SomeField, newValue).ConfigureAwait(false);

            // Assert
            VerifyFieldWasUpdated(rdo, nameof(SampleRdo.SomeField), newValue);
            VerifyFieldWasNotUpdated(nameof(SampleRdo.OptionalTextField));
        }

        [Test]
        public async Task GetAsync_ShouldParseGuidValues()
        {
            // Arrange
            const int objectArtifactId = 70;

            ExtendedSampleRdo sourceRdo = new ExtendedSampleRdo()
            {
                ArtifactId = objectArtifactId,
                GuidField = Guid.NewGuid()
            };

            SetupObjectManagerToReturnRdo(sourceRdo);

            // Act
            ExtendedSampleRdo result = await _sut
                .GetAsync<ExtendedSampleRdo>(WorkspaceId, sourceRdo.ArtifactId, x => x.GuidField)
                .ConfigureAwait(false);

            // Assert
            _ = result.GuidField.Should().Be(sourceRdo.GuidField);
        }

        [TestCase(null)]
        [TestCase("35D34277-B04D-4A26-83B7-DBD95F103B37")]
        public async Task GetAsync_ShouldHandleNullableGuidValues(string guid)
        {
            // Arrange
            const int objectArtifactId = 70;

            ExtendedSampleRdo sourceRdo = new ExtendedSampleRdo()
            {
                ArtifactId = objectArtifactId,
                NullableGuidField = guid == null ? (Guid?)null : Guid.Parse(guid)
            };

            SetupObjectManagerToReturnRdo(sourceRdo);

            // Act
            ExtendedSampleRdo result = await _sut
                .GetAsync<ExtendedSampleRdo>(WorkspaceId, sourceRdo.ArtifactId, x => x.NullableGuidField)
                .ConfigureAwait(false);

            // Assert
            _ = result.GuidField.Should().Be(sourceRdo.GuidField);
        }

        [Test]
        public async Task GetAsync_ShouldStreamTruncatedLongTextFields()
        {
            // Arrange
            const int objectArtifactId = 68;

            // ReSharper disable StringLiteralTypo
            const string textToStream = "Czwartek to mały piątek, na logikę dzisiaj środa, powoli można startować, weekend zaczynać od nowa";

            ExtendedSampleRdo sourceRdo = new ExtendedSampleRdo()
            {
                ArtifactId = objectArtifactId,
                LongTextField = "Lorem ipsum..."
            };

            SetupObjectManagerToReturnRdo(sourceRdo);
            SetupKeplerStreaming(textToStream);

            // Act
            ExtendedSampleRdo result = await _sut
                .GetAsync<ExtendedSampleRdo>(WorkspaceId, sourceRdo.ArtifactId, x => x.LongTextField)
                .ConfigureAwait(false);

            // Assert
            _ = result.LongTextField.Should().Be(textToStream);
        }

        private void SetupFieldManager(Mock<IFieldManager> fieldManagerMock)
        {
            _ = fieldManagerMock.Setup(x => x.CreateYesNoFieldAsync(WorkspaceId, It.IsAny<YesNoFieldRequest>()))
                .ReturnsAsync(CreatedFieldId);
            _ = fieldManagerMock
                .Setup(x => x.CreateWholeNumberFieldAsync(WorkspaceId, It.IsAny<WholeNumberFieldRequest>()))
                .ReturnsAsync(CreatedFieldId);
            _ = fieldManagerMock
                .Setup(x => x.CreateFixedLengthFieldAsync(WorkspaceId, It.IsAny<FixedLengthFieldRequest>()))
                .ReturnsAsync(CreatedFieldId);
            _ = fieldManagerMock.Setup(x => x.CreateLongTextFieldAsync(WorkspaceId, It.IsAny<LongTextFieldRequest>()))
                .ReturnsAsync(CreatedFieldId);
            _ = fieldManagerMock
                .Setup(x => x.CreateSingleObjectFieldAsync(WorkspaceId, It.IsAny<SingleObjectFieldRequest>()))
                .ReturnsAsync(CreatedFieldId);
        }

        private void SetupKeplerStreaming(string textToStream)
        {
            var keplerStreamMock = new Mock<IKeplerStream>();
            var stream = GetStream(textToStream);

            _ = _objectManagerMock.Setup(x =>
                    x.StreamLongTextAsync(WorkspaceId, It.IsAny<RelativityObjectRef>(), It.IsAny<FieldRef>()))
                .ReturnsAsync(keplerStreamMock.Object);

            _ = keplerStreamMock.Setup(x => x.GetStreamAsync()).ReturnsAsync(stream);
        }

        private Stream GetStream(string textToStream)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Unicode);
            writer.Write(textToStream);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void SetupObjectManagerToReturnRdo<TRdo>(TRdo sourceRdo) where TRdo : IRdoType
        {
            var rdoTypeInfo = _rdoGuidProviderMock.Object.GetValue<TRdo>();
            _ = _objectManagerMock.Setup(x => x.QuerySlimAsync(
                    WorkspaceId,
                    It.Is<QueryRequest>(q => q.ObjectType.Guid.Value == rdoTypeInfo.TypeGuid),
                    0,
                    1))
                .ReturnsAsync((int _, QueryRequest request, int __, int ___) => new QueryResultSlim()
                {
                    Objects = new List<RelativityObjectSlim>
                    {
                        new RelativityObjectSlim()
                        {
                            ArtifactID = sourceRdo.ArtifactId,
                            Values = request.Fields

                                // ReSharper disable once PossibleInvalidOperationException
                                .Select(x => rdoTypeInfo.Fields[x.Guid.Value].PropertyInfo)
                                .Select(propertyInfo => (object)propertyInfo.GetValue(sourceRdo)).ToList()
                        }
                    }
                });
        }

        private void VerifyFieldWasQueried(string fieldName, Times times)
        {
            var fieldInfo = SampleRdo.ExpectedRdoInfo.Fields.Values.First(x => x.Name == fieldName);

            _objectManagerMock.Verify(
                x => x.QuerySlimAsync(
                WorkspaceId,
                It.Is<QueryRequest>(q => q.Fields.Any(f => f.Guid == fieldInfo.Guid)),
                0,
                1), times);
        }

        private void VerifyFieldWasUpdated(SampleRdo rdo, string fieldName, object value)
        {
            var fieldInfo = SampleRdo.ExpectedRdoInfo.Fields.Values.First(x => x.Name == fieldName);

            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                    WorkspaceId,
                    It.Is<UpdateRequest>(
                        q => q.FieldValues.Any(f => f.Field.Guid == fieldInfo.Guid && f.Value.Equals(value)))),
                Times.Once);

            _ = fieldInfo.PropertyInfo.GetValue(rdo).Should().Be(value);
        }

        private void VerifyFieldWasNotUpdated(string fieldName)
        {
            var fieldInfo = SampleRdo.ExpectedRdoInfo.Fields.Values.First(x => x.Name == fieldName);

            _objectManagerMock.Verify(
                x => x.UpdateAsync(
                    WorkspaceId,
                    It.Is<UpdateRequest>(q => q.FieldValues.Any(f => f.Field.Guid == fieldInfo.Guid))),
                Times.Never);
        }

        [Rdo("37C93366-5D07-42D0-8EA8-2400B4323F36", nameof(RdoWithParent), "22C0EE5E-4CA6-49D3-86F8-63DF2382083D")]
        private class RdoWithParent : IRdoType
        {
            public int ArtifactId { get; set; }
        }
    }
}
