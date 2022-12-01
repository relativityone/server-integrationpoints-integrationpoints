using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class SyncFieldManagerTests
    {
        private Mock<IDestinationServiceFactoryForAdmin> _serviceFactoryFake;
        private Mock<IArtifactGuidManager> _artifactGuidManagerMock;
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<IFieldManager> _fieldManagerMock;

        private SyncFieldManager _sut;

        private const int _WORKSPACE_ID = 1;
        private const int _FIELD_ARTIFACT_ID = 10;
        private const string _FIELD_NAME = "My Field";
        private readonly Guid _FIELD_GUID = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryFake = new Mock<IDestinationServiceFactoryForAdmin>();
            _artifactGuidManagerMock = new Mock<IArtifactGuidManager>();
            _objectManagerMock = new Mock<IObjectManager>();
            _fieldManagerMock = new Mock<IFieldManager>();
            _serviceFactoryFake.Setup(x => x.CreateProxyAsync<IArtifactGuidManager>()).ReturnsAsync(_artifactGuidManagerMock.Object);
            _serviceFactoryFake.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerMock.Object);
            _serviceFactoryFake.Setup(x => x.CreateProxyAsync<IFieldManager>()).ReturnsAsync(_fieldManagerMock.Object);

            _sut = new SyncFieldManager(_serviceFactoryFake.Object, new EmptyLogger());
        }

        [Test]
        public async Task EnsureFieldsExistAsync_ShouldNotRegisterFieldTypeGuid_WhenExists()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, true);

            // Act
            await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, It.IsAny<Dictionary<Guid, BaseFieldRequest>>()).ConfigureAwait(false);

            // Assert
            _artifactGuidManagerMock.Verify(
                x => x.CreateSingleAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<Guid>>()), Times.Never);
        }

        [Test]
        public async Task EnsureFieldsExistAsync_ShouldQueryExistingFieldByName()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, false);

            SetupFieldRead(_FIELD_NAME, CreateSingleFieldResult(_FIELD_ARTIFACT_ID));

            Dictionary<Guid, BaseFieldRequest> fieldRequest = new Dictionary<Guid, BaseFieldRequest>()
            {
                { _FIELD_GUID, CreateFieldRequest<FixedLengthFieldRequest>(_FIELD_NAME) }
            };

            // Act
            await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, fieldRequest).ConfigureAwait(false);

            // Assert
            _objectManagerMock.Verify();
        }

        [Test]
        public async Task EnsureFieldsExistAsync_ShouldAssignGuid()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, false);

            SetupFieldRead(_FIELD_NAME, CreateSingleFieldResult(_FIELD_ARTIFACT_ID));

            Dictionary<Guid, BaseFieldRequest> fieldRequest = new Dictionary<Guid, BaseFieldRequest>()
            {
                { _FIELD_GUID, CreateFieldRequest<FixedLengthFieldRequest>(_FIELD_NAME) }
            };

            // Act
            await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, fieldRequest).ConfigureAwait(false);

            // Assert
            _objectManagerMock.Verify();
            _artifactGuidManagerMock.Verify(x => x.CreateSingleAsync(_WORKSPACE_ID, _FIELD_ARTIFACT_ID,
                It.Is<List<Guid>>(list => list.Contains(_FIELD_GUID))));
        }

        [Test]
        public async Task EnsureFieldsExistAsync_ShouldCreateNewWholeNumberField()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, false);

            _objectManagerMock.Setup(x => x.QueryAsync(_WORKSPACE_ID, GetFieldQueryRequest(_FIELD_NAME),
                    It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(CreateEmptyResult()).Verifiable();

            WholeNumberFieldRequest expectedFieldRequest = CreateFieldRequest<WholeNumberFieldRequest>(_FIELD_NAME);

            Dictionary<Guid, BaseFieldRequest> fieldRequest = new Dictionary<Guid, BaseFieldRequest>()
            {
                { _FIELD_GUID, expectedFieldRequest }
            };

            // Act
            await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, fieldRequest).ConfigureAwait(false);

            // Assert
            _fieldManagerMock.Verify(x => x.CreateWholeNumberFieldAsync(_WORKSPACE_ID, expectedFieldRequest));
        }

        [Test]
        public async Task EnsureFieldsExistAsync_ShouldCreateNewFixedLengthTextField()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, false);

            SetupFieldRead(_FIELD_NAME, CreateEmptyResult());

            FixedLengthFieldRequest expectedFieldRequest = CreateFieldRequest<FixedLengthFieldRequest>(_FIELD_NAME);

            Dictionary<Guid, BaseFieldRequest> fieldRequest = new Dictionary<Guid, BaseFieldRequest>()
            {
                { _FIELD_GUID, expectedFieldRequest }
            };

            // Act
            await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, fieldRequest).ConfigureAwait(false);

            // Assert
            _objectManagerMock.Verify();
            _fieldManagerMock.Verify(x => x.CreateFixedLengthFieldAsync(_WORKSPACE_ID, expectedFieldRequest));
        }

        [Test]
        public async Task EnsureFieldsExistAsync_ShouldCreateNewMultipleObjectField()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, false);

            SetupFieldRead(_FIELD_NAME, CreateEmptyResult());

            MultipleObjectFieldRequest expectedFieldRequest = CreateFieldRequest<MultipleObjectFieldRequest>(_FIELD_NAME);

            Dictionary<Guid, BaseFieldRequest> fieldRequest = new Dictionary<Guid, BaseFieldRequest>()
            {
                { _FIELD_GUID, expectedFieldRequest }
            };

            // Act
            await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, fieldRequest).ConfigureAwait(false);

            // Assert
            _objectManagerMock.Verify();
            _fieldManagerMock.Verify(x => x.CreateMultipleObjectFieldAsync(_WORKSPACE_ID, expectedFieldRequest));
        }

        [Test]
        public async Task EnsureFieldsExistAsync_ShouldTryReadFieldAgain_WhenFirstReadNotFoundAndCreationFailedDueFieldExists()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, false);

            _objectManagerMock.SetupSequence(x => x.QueryAsync(
                _WORKSPACE_ID,
                GetFieldQueryRequest(_FIELD_NAME), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(CreateEmptyResult())
                .ReturnsAsync(CreateSingleFieldResult(_FIELD_ARTIFACT_ID));

            _fieldManagerMock.Setup(x => x.CreateWholeNumberFieldAsync(
                It.IsAny<int>(),
                It.IsAny<WholeNumberFieldRequest>()))
                .Throws<InvalidInputException>();

            Dictionary<Guid, BaseFieldRequest> fieldRequest = new Dictionary<Guid, BaseFieldRequest>()
            {
                { _FIELD_GUID, CreateFieldRequest<WholeNumberFieldRequest>(_FIELD_NAME) }
            };

            // Act
            await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, fieldRequest).ConfigureAwait(false);

            // Assert
            _artifactGuidManagerMock.Verify(
                x => x.CreateSingleAsync(_WORKSPACE_ID, _FIELD_ARTIFACT_ID,
                new List<Guid>() { _FIELD_GUID }), Times.Once);
        }

        [Test]
        public void EnsureFieldsExistAsync_ShouldThrowException_WhenCreatingUnsupportedFieldType()
        {
            // Arrange
            SetupFieldGuidExists(_FIELD_GUID, false);

            SetupFieldRead(_FIELD_NAME, CreateEmptyResult());

            Dictionary<Guid, BaseFieldRequest> fieldRequest = new Dictionary<Guid, BaseFieldRequest>()
            {
                { _FIELD_GUID, CreateFieldRequest<SingleChoiceFieldRequest>(_FIELD_NAME) }
            };

            // Act
            Func<Task> action = async () => await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, fieldRequest).ConfigureAwait(false);

            // Assert
            action.Should().Throw<NotSupportedException>();
        }

        [Test]
        public void EnsureFieldsExistAsync_ShouldNotThrow_WhenPassingNullDictionary()
        {
            // Act
            Func<Task> action = async () => await _sut.EnsureFieldsExistAsync(_WORKSPACE_ID, null).ConfigureAwait(false);

            // Assert
            action.Should().NotThrow();
        }

        private void SetupFieldGuidExists(Guid fieldGuid, bool exists)
        {
            _artifactGuidManagerMock.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, fieldGuid)).ReturnsAsync(exists);
        }

        private void SetupFieldRead(string fieldName, QueryResult result)
        {
            _objectManagerMock.Setup(x => x.QueryAsync(
                _WORKSPACE_ID,
                GetFieldQueryRequest(fieldName), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(result)
                .Verifiable();
        }

        private QueryRequest GetFieldQueryRequest(string fieldName)
        {
            return It.Is<QueryRequest>(request =>
                request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
                request.Condition.Contains(fieldName));
        }

        private QueryResult CreateEmptyResult() => new QueryResult { Objects = new List<RelativityObject>() };

        private QueryResult CreateSingleFieldResult(int fieldArtifactId) =>
            new QueryResult { Objects = new List<RelativityObject>() { new RelativityObject { ArtifactID = fieldArtifactId } } };

        private T CreateFieldRequest<T>(string fieldName) where T : BaseFieldRequest, new()
        {
            T request = new T();

            request.Name = fieldName;

            return request;
        }
    }
}
