using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class ConfigurationTests
    {
        private IAPILog _syncLog;
        private Mock<ISemaphoreSlim> _semaphoreSlim;
        private SyncJobParameters _syncJobParameters;
        private Mock<IRdoManager> _rdoManagerMock;
        private SyncConfigurationRdo _syncConfigurationRdo;
        private IConfiguration _sut;

        private const int _TEST_WORKSPACE_ID = 789;
        private const int _TEST_USER_ID = 5677;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _syncLog = new EmptyLogger();
            _syncJobParameters = new SyncJobParameters(It.IsAny<int>(), _TEST_WORKSPACE_ID, _TEST_USER_ID, It.IsAny<Guid>(), Guid.Empty);
        }

        [SetUp]
        public async Task SetUp()
        {
            _syncConfigurationRdo = new SyncConfigurationRdo();
            _semaphoreSlim = new Mock<ISemaphoreSlim>();
            _rdoManagerMock = new Mock<IRdoManager>();
            
            _rdoManagerMock.Setup(x => x.GetAsync<SyncConfigurationRdo>(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_syncConfigurationRdo);
        
            _sut = await Sync.Storage.Configuration.GetAsync(_syncJobParameters, _syncLog, _semaphoreSlim.Object, _rdoManagerMock.Object).ConfigureAwait(false);
        }

        [Test]
        public void ItShouldReadFields()
        {
            // ARRANGE
            _syncConfigurationRdo.JobHistoryId = 5;
            _syncConfigurationRdo.SnapshotId = new Guid();


            // ACT && ASSERT
            _sut.GetFieldValue(x => x.JobHistoryId).Should().Be(_syncConfigurationRdo.JobHistoryId);
            _sut.GetFieldValue(x => x.SnapshotId).Should().Be(_syncConfigurationRdo.SnapshotId);

            _sut.GetFieldValue(x => x.JobHistoryToRetryId).Should().Be(default(int?));
        }

        
        [Test]
        public void ItShouldFailWhenConfigurationNotFound()
        {
            // ARRANGE
            _rdoManagerMock.Setup(x => x.GetAsync<SyncConfigurationRdo>(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((SyncConfigurationRdo)null);

            // ACT
            Func<Task> action = () => Sync.Storage.Configuration.GetAsync(_syncJobParameters, _syncLog, _semaphoreSlim.Object, _rdoManagerMock.Object);

            // ASSERT
            action.Should().Throw<SyncException>();
        }


        [Test]
        public async Task ItShouldUpdateField()
        {
            // ARRANGE
            const int newValue = 200;

            _rdoManagerMock
                .Setup(x => x.SetValueAsync(_TEST_WORKSPACE_ID, _syncConfigurationRdo, It.IsAny<Expression<Func<SyncConfigurationRdo, int>>>(), It.IsAny<int>()))
                .Callback((int ws, SyncConfigurationRdo rdo, Expression<Func<SyncConfigurationRdo, int>> expression, int value) =>
                {
                    rdo.JobHistoryId = value;
                })
                .Returns(Task.CompletedTask);

            // ACT
            await _sut.UpdateFieldValueAsync(x => x.JobHistoryId, newValue).ConfigureAwait(false);

            // ASSERT
            _syncConfigurationRdo.JobHistoryId.Should().Be(newValue);

            _rdoManagerMock.Verify(x => x.SetValueAsync(_TEST_WORKSPACE_ID, It.IsAny<SyncConfigurationRdo>(), r => r.JobHistoryId, newValue));
        }

        [Test]
        public void ItShouldNotSetNewValueWhenUpdateFails()
        {
            // ARRANGE
            const int newValue = 200;

            _rdoManagerMock.Setup(x => x.SetValueAsync(_TEST_WORKSPACE_ID, It.IsAny<SyncConfigurationRdo>(), r => r.JobHistoryId, newValue))
                .Throws<InvalidOperationException>();
        
            // ACT
            Func<Task> action = () => _sut.UpdateFieldValueAsync(x => x.JobHistoryId, newValue);

            // ASSERT
            action.Should().Throw<InvalidOperationException>();

            _sut.GetFieldValue(x => x.JobHistoryId).Should().Be(_syncConfigurationRdo.JobHistoryId);
        }

        [Test]
        public void ItShouldDisposeSemaphore()
        {
            // ACT
            _sut.Dispose();

            // ASSERT
            _semaphoreSlim.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
