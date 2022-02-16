using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
    [TestFixture]
    public class SnapshotValidatorTests
    {
        private const int WORKSPACE_ID = 5;
        
        private Mock<IObjectManager> _objectManagerMock;
        private ConfigurationStub _configuration;
        private SnapshotValidator _sut;
        private Mock<ISourceServiceFactoryForAdmin> _syncServiceManagerMock;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManager>();
            _syncServiceManagerMock = new Mock<ISourceServiceFactoryForAdmin>();

            _syncServiceManagerMock.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .Returns(Task.FromResult(_objectManagerMock.Object));
            
            _configuration = new ConfigurationStub();

            _sut = new SnapshotValidator(_configuration, _syncServiceManagerMock.Object);
        }
        
        [TestCaseSource(nameof(SnapshotCaseSource))]
        public async Task Validate_ShouldReturnExpectedValue(Guid? snapshotId, RelativityObjectSlim[] exportResult, bool expectedValue)
        {
            // Arrange
            _configuration.SourceWorkspaceArtifactId = WORKSPACE_ID;
            _configuration.SnapshotId = snapshotId;
            _objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(WORKSPACE_ID, It.IsAny<Guid>(),1,0))
                .ReturnsAsync(exportResult);
            
            // Act
            ValidationResult result =
                await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);
            
            // Assert
            result.IsValid.Should().Be(expectedValue);

            if (snapshotId != null)
            {
                _objectManagerMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(WORKSPACE_ID, snapshotId.Value,1, 0), Times.Once);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShouldValidate_ShouldRespectResumingFromConfiguration(bool resuming)
        {
            // Arrange
            _configuration.Resuming = resuming;
            
            // Act && Assert
            _sut.ShouldValidate(null).Should().Be(resuming);
        }
        
        static IEnumerable<TestCaseData> SnapshotCaseSource()
        {
            yield return new TestCaseData((Guid?) null, null, false);
            yield return new TestCaseData((Guid?) Guid.Empty, null, false);
            yield return new TestCaseData((Guid?) Guid.NewGuid(), new RelativityObjectSlim[0], true);
            yield return new TestCaseData((Guid?) Guid.NewGuid(), null, false);
        }
    }
}