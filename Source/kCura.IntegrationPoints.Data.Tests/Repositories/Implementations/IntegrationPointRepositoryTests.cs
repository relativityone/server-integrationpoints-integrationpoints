using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointRepositoryTests
    {
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private Mock<IIntegrationPointSerializer> _serializerMock;
        private Mock<ISecretsRepository> _secretsRepositoryMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IAPILog> _internalLoggerMock;
        private IntegrationPoint _integrationPoint;
        private IEnumerable<FieldMap> _fieldMapping;
        private IEnumerable<FieldMap> _emptyFieldMapping;
        private Stream _fieldMappingStream;
        private Stream _fieldMappingInvalidStream;
        private Stream _fieldMappingEmptyStream;
        private Guid _guid = Guid.Parse(IntegrationPointFieldGuids.FieldMappings);
        private DateTime _nextScheduledRuntime;
        private DateTime _lastRuntime;
        private IntegrationPointRepository _sut;

        private const int _SOURCE_PROVIDER = 2;
        private const int _DESTINATION_PROVIDER = 4;
        private const int _TYPE = 3;
        private const int _JOB_HISTORY_1 = 12;
        private const int _JOB_HISTORY_2 = 15;
        private const int _ARTIFACT_ID = 1025823;
        private const string _FIELD_MAPPING_LONG = "fieldMappingLong";
        private const string _FIELD_MAPPING_INVALID = "fieldMappingInvalid";
        private const string _SECURED_CONFIGURATION = "2ddbfb34-5dce-47a2-99ab-a7328600673b";
        private const string _NAME = "Test Integration Point";

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IRelativityObjectManager>();
            _serializerMock = new Mock<IIntegrationPointSerializer>();
            _loggerMock = new Mock<IAPILog>();
            _internalLoggerMock = new Mock<IAPILog>();
            _secretsRepositoryMock = new Mock<ISecretsRepository>();
            _loggerMock.Setup(x => x.ForContext<IntegrationPointRepository>()).Returns(_internalLoggerMock.Object);
            _fieldMapping = CreateFieldMapping();
            _emptyFieldMapping = new List<FieldMap>();
            _fieldMappingStream = GenerateStreamFromString(_FIELD_MAPPING_LONG);
            _fieldMappingInvalidStream = GenerateStreamFromString(_FIELD_MAPPING_INVALID);
            _fieldMappingEmptyStream = GenerateStreamFromString(string.Empty);
            _serializerMock.Setup(x => x.Deserialize<IEnumerable<FieldMap>>(_FIELD_MAPPING_LONG)).Returns(_fieldMapping);
            _serializerMock.Setup(x => x.Deserialize<IEnumerable<FieldMap>>(_FIELD_MAPPING_INVALID))
                .Throws<SerializationException>();
            
            _nextScheduledRuntime = DateTime.UtcNow.AddDays(1);
            _lastRuntime = DateTime.UtcNow.AddDays(-1);
            _sut = new IntegrationPointRepository(
                _objectManagerMock.Object, 
                _serializerMock.Object,
                _secretsRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Read_ShouldReturnValidIntegrationPoint()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);
            _objectManagerMock.Setup(x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser))
                .Returns(_fieldMappingStream);
            IntegrationPoint expectedResult = CreateTestIntegrationPoint();
            expectedResult.FieldMappings = _FIELD_MAPPING_LONG;

            // Act
            IntegrationPoint actualResult = _sut.ReadWithFieldMappingAsync(_ARTIFACT_ID).GetAwaiter().GetResult();

            // Assert
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser),
                Times.Once());
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(), 
                    It.IsAny<string>(), 
                    It.IsAny<object[]>()),
                Times.Never);
            AreIntegrationPointsEqual(expectedResult, actualResult).Should().BeTrue();
        }

        [Test]
        public async Task ReadEncryptedAsync_ShouldReturnValidIntegrationPoint()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID,ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);

            // Act
            IntegrationPoint actualResult = await _sut.ReadEncryptedAsync(_ARTIFACT_ID).ConfigureAwait(false);

            // Assert
            _secretsRepositoryMock.Verify(x => x.DecryptAsync(It.IsAny<SecretPath>()), Times.Never);
            _objectManagerMock.Verify(x => x.StreamUnicodeLongText(It.IsAny<int>(), It.IsAny<FieldRef>(), ExecutionIdentity.CurrentUser), Times.Never);
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
            actualResult.Should().Be(_integrationPoint);
        }

        [Test]
        public void Read_ShouldThrowException_WhenObjectManagerReadThrowsException()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Throws<Exception>();

            // Act
            Action action = () => _sut.ReadWithFieldMappingAsync(_ARTIFACT_ID).GetAwaiter().GetResult();

            // Assert
            action.ShouldThrow<Exception>();
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    It.IsAny<int>(),
                    It.IsAny<FieldRef>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Never);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Test]
        public void Read_ShouldThrowException_WhenObjectManagerStreamLongTextAsyncThrowsException()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);
            _objectManagerMock.Setup(x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser))
                .Throws<Exception>();

            // Act
            Action action = () => _sut.ReadWithFieldMappingAsync(_ARTIFACT_ID).GetAwaiter().GetResult();

            // Assert
            action.ShouldThrow<Exception>();
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    It.IsAny<int>(),
                    It.IsAny<FieldRef>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Once);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Test]
        public void GetFieldMapJsonAsync_ShouldReturnValidFieldMapping()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser))
                .Returns(_fieldMappingStream);

            // Act
            IEnumerable<FieldMap> actualResult = _sut.GetFieldMappingAsync(_ARTIFACT_ID).GetAwaiter().GetResult();

            // Assert
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser),
                Times.Once());
            _serializerMock.Verify(x => x.Deserialize<IEnumerable<FieldMap>>(_FIELD_MAPPING_LONG), Times.Once);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
            actualResult.Should().Equal(_fieldMapping);
        }

        [Test]
        public void GetFieldMapJsonAsync_ShouldReturnEmptyFieldMapping_WhenWorkspaceArtifactIDIsZero()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();

            // Act
            IEnumerable<FieldMap> actualResult = _sut.GetFieldMappingAsync(0).GetAwaiter().GetResult();

            // Assert
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    It.IsAny<int>(),
                    It.IsAny<FieldRef>(),
                    It.IsAny<ExecutionIdentity>()),
                Times.Never);
            _serializerMock.Verify(x => x.Deserialize<IEnumerable<FieldMap>>(It.IsAny<string>()), Times.Never);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
            actualResult.Should().Equal(_emptyFieldMapping);
        }

        [Test]
        public void GetFieldMapJsonAsync_ShouldReturnEmptyFieldMapping_WhenFieldMappingJsonIsEmpty()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser))
                .Returns(_fieldMappingEmptyStream);

            // Act
            IEnumerable<FieldMap> actualResult = _sut.GetFieldMappingAsync(_ARTIFACT_ID).GetAwaiter().GetResult();

            // Assert
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser),
                Times.Once());
            _serializerMock.Verify(x => x.Deserialize<IEnumerable<FieldMap>>(It.IsAny<string>()), Times.Never);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
            actualResult.Should().Equal(_emptyFieldMapping);
        }

        [Test]
        public void GetFieldMapJsonAsync_ShouldThrowException_WhenObjectManagerStreamLongTextAsyncThrowsException()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser))
                .Throws<Exception>();

            // Act
            Action action = () => _sut.GetFieldMappingAsync(_ARTIFACT_ID).GetAwaiter().GetResult();

            // Assert
            action.ShouldThrow<Exception>();
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser),
                Times.Once());
            _serializerMock.Verify(x => x.Deserialize<IEnumerable<FieldMap>>(_FIELD_MAPPING_LONG), Times.Never);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Test]
        public void GetFieldMapJsonAsync_ShouldThrowException_WhenFieldMappingIsInvalid()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser))
                .Returns(_fieldMappingInvalidStream);

            // Act
            Action action = () => _sut.GetFieldMappingAsync(_ARTIFACT_ID).GetAwaiter().GetResult();

            // Assert
            action.ShouldThrow<SerializationException>();
            _objectManagerMock.Verify(
                x => x.StreamUnicodeLongText(
                    _ARTIFACT_ID,
                    It.Is<FieldRef>(f => f.Guid.ToString() == _guid.ToString()),
                    ExecutionIdentity.CurrentUser),
                Times.Once());
            _serializerMock.Verify(x => x.Deserialize<IEnumerable<FieldMap>>(_FIELD_MAPPING_INVALID), Times.Once);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<SerializationException>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Test]
        public void This_ShouldThrowException_WhenObjectManagerIsNull()
        {
            // Act
            Action action = () => new IntegrationPointRepository(
                null,
                _serializerMock.Object,
                _secretsRepositoryMock.Object,
                _loggerMock.Object);

            // Assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void GetSecuredConfiguration_ShouldReturnValidSecuredConfiguration()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);

            // Act
            string actualResult = _sut.GetSecuredConfiguration(_ARTIFACT_ID);

            // Assert
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
            actualResult.Should().Be(_SECURED_CONFIGURATION);
        }

        [Test]
        public void GetSecuredConfiguration_ShouldThrowException_WhenObjectManagerReadThrowsException()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Throws<Exception>();

            // Act
            Action action = () => _sut.GetSecuredConfiguration(_ARTIFACT_ID);

            // Assert
            action.ShouldThrow<Exception>();
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
        }

        [Test]
        public void GetName_ShouldReturnValidName()
        {
            // Arrange
            _integrationPoint = CreateTestIntegrationPoint();
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Returns(_integrationPoint);

            // Act
            string actualResult = _sut.GetName(_ARTIFACT_ID);

            // Assert
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
            actualResult.Should().Be(_NAME);
        }

        [Test]
        public void GetName_ShouldThrowException_WhenObjectManagerReadThrowsException()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser)).Throws<Exception>();

            // Act
            Action action = () => _sut.GetName(_ARTIFACT_ID);

            // Assert
            action.ShouldThrow<Exception>();
            _objectManagerMock.Verify(x => x.Read<IntegrationPoint>(_ARTIFACT_ID, ExecutionIdentity.CurrentUser), Times.Once);
            _internalLoggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private static IEnumerable<FieldMap> CreateFieldMapping()
        {
            var sourceField = new FieldEntry
            {
                DisplayName = "Control Number",
                FieldIdentifier = "1000123",
                FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String,
                IsIdentifier = true,
                IsRequired = false,
                Type = "Long Text"
            };

            var destinationField = new FieldEntry
            {
                DisplayName = "Control Number",
                FieldIdentifier = "1000456",
                FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String,
                IsIdentifier = true,
                IsRequired = false,
                Type = "Long Text"
            };

            var fieldMap = new FieldMap
            {
                SourceField = sourceField,
                DestinationField = destinationField,
                FieldMapType = FieldMapTypeEnum.Identifier
            };

            return new List<FieldMap> {fieldMap};
        }

        private IntegrationPoint CreateTestIntegrationPoint()
        {
            return new IntegrationPoint
            {
                ArtifactId = _ARTIFACT_ID,
                Name = _NAME,
                SourceConfiguration = "sourceConf",
                DestinationConfiguration = "destConf",
                FieldMappings = "fieldMappingsShort",
                SecuredConfiguration = _SECURED_CONFIGURATION,
                SourceProvider = _SOURCE_PROVIDER,
                DestinationProvider = _DESTINATION_PROVIDER,
                Type = _TYPE,
                EmailNotificationRecipients = "admin@relativity.com",
                ScheduleRule = "scheduleRule",
                EnableScheduler = true,
                HasErrors = false,
                LogErrors = true,
                OverwriteFields = null,
                JobHistory = new[] { _JOB_HISTORY_1, _JOB_HISTORY_2 },
                NextScheduledRuntimeUTC = _nextScheduledRuntime,
                LastRuntimeUTC = _lastRuntime,
            };
        }

        private static Stream GenerateStreamFromString(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static bool AreIntegrationPointsEqual(IntegrationPoint integrationPoint1, IntegrationPoint integrationPoint2)
        {
            return integrationPoint1.ArtifactId == integrationPoint2.ArtifactId &&
                   integrationPoint1.Name == integrationPoint2.Name &&
                   integrationPoint1.SourceConfiguration == integrationPoint2.SourceConfiguration &&
                   integrationPoint1.DestinationConfiguration == integrationPoint2.DestinationConfiguration &&
                   integrationPoint1.FieldMappings == integrationPoint2.FieldMappings &&
                   integrationPoint1.SecuredConfiguration == integrationPoint2.SecuredConfiguration &&
                   integrationPoint1.SourceProvider == integrationPoint2.SourceProvider &&
                   integrationPoint1.DestinationProvider == integrationPoint2.DestinationProvider &&
                   integrationPoint1.Type == integrationPoint2.Type &&
                   integrationPoint1.EmailNotificationRecipients == integrationPoint2.EmailNotificationRecipients &&
                   integrationPoint1.ScheduleRule == integrationPoint2.ScheduleRule &&
                   integrationPoint1.EnableScheduler == integrationPoint2.EnableScheduler &&
                   integrationPoint1.HasErrors == integrationPoint2.HasErrors &&
                   integrationPoint1.LogErrors == integrationPoint2.LogErrors &&
                   integrationPoint1.OverwriteFields == integrationPoint2.OverwriteFields &&
                   integrationPoint1.JobHistory.SequenceEqual(integrationPoint2.JobHistory) &&
                   Equals(integrationPoint1.NextScheduledRuntimeUTC, integrationPoint2.NextScheduledRuntimeUTC) &&
                   Equals(integrationPoint1.LastRuntimeUTC, integrationPoint2.LastRuntimeUTC);
        }
    }
}