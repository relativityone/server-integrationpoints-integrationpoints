using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
    public class FieldConfigurationTests
    {
        private const int _WORKSPACE_ID = 111;
        private const int _SYNC_CONFIG_ID = 222;
        private const int _USER_ID = 333;
        private readonly Guid _workflowId = Guid.NewGuid();

        private Mock<IConfiguration> _cacheFake;
        private Mock<IFieldMappings> _fieldMappingsMock;
        private SyncJobParameters _syncJobParameters;

        private FieldConfiguration _sut;

        [SetUp]
        public void SetUp()
        {
            _cacheFake = new Mock<IConfiguration>();
            _fieldMappingsMock = new Mock<IFieldMappings>();
            _syncJobParameters = new SyncJobParameters(_SYNC_CONFIG_ID, _WORKSPACE_ID, _USER_ID, _workflowId, Guid.Empty);

            _sut = PrepareSut();
        }

        [Test]
        public void SourceWorkspaceArtifactId_ShouldReturnSourceWorkspaceArtifactId()
        {
            // act
            int sourceWorkspaceId = _sut.SourceWorkspaceArtifactId;

            // assert
            sourceWorkspaceId.Should().Be(_WORKSPACE_ID);
        }

        [Test]
        public void GetFolderPathSourceFieldName_ShouldReturnValueFromCache()
        {
            // arrange
            const string fieldName = "abc";
            _cacheFake.Setup(cache => cache.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>())).Returns(fieldName);

            // act
            string actualFieldName = _sut.GetFolderPathSourceFieldName();

            // assert
            actualFieldName.Should().Be(fieldName);
        }

        [Test]
        public void GetFieldMappings_ShouldReturnFieldMappings()
        {
            // act
            _sut.GetFieldMappings();

            // assert
            _fieldMappingsMock.Verify(x => x.GetFieldMappings());
        }

        private FieldConfiguration PrepareSut()
        {
            return new FieldConfiguration(_cacheFake.Object, _fieldMappingsMock.Object, _syncJobParameters);
        }
    }
}
