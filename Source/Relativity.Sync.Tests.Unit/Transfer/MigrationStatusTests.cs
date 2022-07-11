using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.ResourceServer;
using Relativity.Storage;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer.ADF;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal class MigrationStatusTests
	{
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdminMock;
		private Mock<IInstanceSettings> _instanceSettingsMock;
		private Mock<IAPILog> _loggerMock;		
		private Mock<IFileShareServerManager> _fileShareServerManagerMock;
		private Mock<IStorageAccessFactory> _storageAccessFactoryMock;
		private Mock<IStorageDiscovery> _storageDiscoveryMock;
		private MigrationStatusAsync _sut;
		private string _fileshare1Name = "_fileshare1Name";
		private string _fileshare1UNC = "//files/file1";
		private string _fileshare2UNC = "//files/file2";

		[SetUp]
		public void SetUp()
		{
			_serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
			_instanceSettingsMock = new Mock<IInstanceSettings>();
			_loggerMock = new Mock<IAPILog>();
			_fileShareServerManagerMock = new Mock<IFileShareServerManager>();
			_serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IFileShareServerManager>())
				.ReturnsAsync(_fileShareServerManagerMock.Object);

			_storageAccessFactoryMock = new Mock<IStorageAccessFactory>();

			var result1 = new Result<FileShareResourceServer>
			{
				Artifact = new FileShareResourceServer()
				{
					Name = _fileshare1Name,
					UNCPath = _fileshare1UNC
				}
			};
			 
			var resultsSet = new FileShareQueryResultSet()
			{
				Results = new List<Result<FileShareResourceServer>>
				{
					result1
				}
			};
			
			_fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>())).ReturnsAsync(resultsSet);
			_storageDiscoveryMock = new Mock<IStorageDiscovery>();
			_storageAccessFactoryMock.Setup(x => x.CreateStorageDiscoveryAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(_storageDiscoveryMock.Object);
			
			StorageEndpoint resultsBedrock1 = new StorageEndpoint
			{
				EndpointFqdn = _fileshare2UNC,
				StorageInterface = StorageInterface.Adls2,
				StorageAccount = default,
				PrimaryStorageContainer = null,
			};
			StorageEndpoint[] resultFromBedrock = { resultsBedrock1 };
			_storageDiscoveryMock.Setup(x =>
					x.GetStorageEndpointsAsync(It.IsAny<R1Environment>(), It.IsAny<Guid>(),
						It.IsAny<CancellationToken>()))
				.ReturnsAsync(resultFromBedrock);
			
			_sut = new MigrationStatusAsync(_serviceFactoryForAdminMock.Object, _instanceSettingsMock.Object, _storageAccessFactoryMock.Object, _loggerMock.Object);
		}

		[Test]
		public async Task MigrationStatusAsync_ShouldReturnFalse_WhenThereIsAtLeastOneLegacyFileShare()
		{
			bool isTenantFullyMigrated = await _sut.IsTenantFullyMigratedAsync().ConfigureAwait(false);
			isTenantFullyMigrated.Should().BeFalse();
		}
		
		[Test]
		public async Task MigrationStatusAsync_ShouldReturnTrue_WhenThereIsZeroLegacyFileShare()
		{
			var emptyResultsSet = new FileShareQueryResultSet()
			{
				Results = new List<Result<FileShareResourceServer>>()
			};
			
			_fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>())).ReturnsAsync(emptyResultsSet);
			bool isTenantFullyMigrated = await _sut.IsTenantFullyMigratedAsync().ConfigureAwait(false);
			isTenantFullyMigrated.Should().BeTrue();
		}
	}
}