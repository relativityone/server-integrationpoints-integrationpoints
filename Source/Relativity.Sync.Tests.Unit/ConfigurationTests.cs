using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ConfigurationTests
	{
		private ISyncLog _syncLog;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISemaphoreSlim> _semaphoreSlim;
		private Mock<ISourceServiceFactoryForAdmin> _sourceServiceFactoryForAdmin;
		private SyncJobParameters _syncJobParameters;
		private RdoGuidProvider _rdoGuidProvider;
		private Mock<IRdoManager> _rdoManagerMock;
		private SyncConfigurationRdo _syncConfigurationRdo;
		private IConfiguration _sut;

		private const int _TEST_CONFIG_ARTIFACT_ID = 123;
		private const int _TEST_WORKSPACE_ID = 789;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_syncLog = new EmptyLogger();
			_syncJobParameters = new SyncJobParameters(_TEST_CONFIG_ARTIFACT_ID, _TEST_WORKSPACE_ID, 1);
		}

		[SetUp]
		public async Task SetUp()
		{
			_semaphoreSlim = new Mock<ISemaphoreSlim>();
			_objectManager = new Mock<IObjectManager>();
			_sourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_sourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			
			_rdoGuidProvider = new RdoGuidProvider();
			_rdoManagerMock = new Mock<IRdoManager>();

			_syncConfigurationRdo = new SyncConfigurationRdo();
			_rdoManagerMock.Setup(x => x.GetAsync<SyncConfigurationRdo>(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(_syncConfigurationRdo);
			
			_sut = await Sync.Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object, _rdoGuidProvider, _rdoManagerMock.Object).ConfigureAwait(false);
		}

		[Test]
		public void ItShouldReadFields()
		{
			// ARRANGE
			_syncConfigurationRdo.JobHistoryId = 5;
			_syncConfigurationRdo.SnapshotId = "snapshot";


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
			Func<Task> action = async () => await Sync.Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object, _rdoGuidProvider, _rdoManagerMock.Object).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
		}

	
		[Test]
		public async Task ItShouldUpdateField()
		{
			// ARRANGE
			const int newValue = 200;

			// ACT
			await _sut.UpdateFieldValueAsync(x => x.JobHistoryId, newValue).ConfigureAwait(false);

			// ASSERT
			_syncConfigurationRdo.JobHistoryId.Should().Be(newValue);
			
			_rdoManagerMock.Verify(x=> x.SetValuesAsync(_TEST_WORKSPACE_ID, It.Is<SyncConfigurationRdo>(r => r.JobHistoryId == newValue), r => r.JobHistoryId));
		}

		[Test]
		public void ItShouldNotSetNewValueWhenUpdateFails()
		{
			// ARRANGE
			const int newValue = 200;

			_rdoManagerMock.Setup(x => x.SetValuesAsync(_TEST_WORKSPACE_ID,
					It.Is<SyncConfigurationRdo>(c => c.JobHistoryId == newValue), r => r.JobHistoryId))
				.ThrowsAsync(new InvalidOperationException());
		
			// ACT
			Func<Task> action = async () => await _sut.UpdateFieldValueAsync(x => x.JobHistoryId, newValue).ConfigureAwait(false);

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