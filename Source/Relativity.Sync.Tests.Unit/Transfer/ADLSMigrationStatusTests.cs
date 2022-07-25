﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.ResourceServer;
using Relativity.Storage;
using Relativity.Storage.Extensions.Models;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer.ADF;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal class ADLSMigrationStatusTests
    {
        private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdminMock;
        private Mock<IHelperWrapper> _helperFactoryMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IFileShareServerManager> _fileShareServerManagerMock;
        private ADLSMigrationStatus _sut;
        private string _fileshare1Name = @"\\files.t035.ctus014128.r1.kcura.com\T035\Files\";
        private string _fileshare1UNC = @"\\files.t035.ctus014128.r1.kcura.com\T035\Files\";
        private string _fileshare2Name = @"\\files2.t035.ctus014128.r1.kcura.com\T035\Files\";
        private string _fileshare2UNC = @"\\files2.t035.ctus014128.r1.kcura.com\T035\Files\";
        private string _fileshare1BedrockFQDN = "files2.t035.ctus014128.r1.kcura.com";

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
            _loggerMock = new Mock<IAPILog>();
            _fileShareServerManagerMock = new Mock<IFileShareServerManager>();
            _serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IFileShareServerManager>())
                .ReturnsAsync(_fileShareServerManagerMock.Object);

            var fileshareResult1 = new Result<FileShareResourceServer>
            {
                Artifact = new FileShareResourceServer()
                {
                    Name = _fileshare1Name,
                    UNCPath = _fileshare1UNC
                }
            };
            var fileshareResult2 = new Result<FileShareResourceServer>
            {
                Artifact = new FileShareResourceServer()
                {
                    Name = _fileshare2Name,
                    UNCPath = _fileshare2UNC
                }
            };

            var resultsSetForFileServers = new FileShareQueryResultSet
            {
                Results = new List<Result<FileShareResourceServer>>
                {
                    fileshareResult1,
                    fileshareResult2
                }
            };

            _fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
                .ReturnsAsync(resultsSetForFileServers);

            StorageEndpoint resultsBedrock1 = new StorageEndpoint
            {
                EndpointFqdn = _fileshare1BedrockFQDN,
                StorageInterface = StorageInterface.Adls2,
                StorageAccount = default,
                PrimaryStorageContainer = null,
            };
            StorageEndpoint[] resultFromBedrock = { resultsBedrock1 };
            _helperFactoryMock = new Mock<IHelperWrapper>();

            _helperFactoryMock.Setup(x =>
                x.GetStorageEndpointsAsync(It.IsAny<ApplicationDetails>())).ReturnsAsync(resultFromBedrock);

            _sut = new ADLSMigrationStatus(_serviceFactoryForAdminMock.Object, _helperFactoryMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task MigrationStatusAsync_ShouldReturnFalse_WhenThereIsAtLeastOneLegacyFileShare()
        {
            bool isTenantFullyMigrated = await _sut.IsTenantFullyMigratedAsync().ConfigureAwait(false);
            isTenantFullyMigrated.Should().BeFalse();
        }

        [Test]
        public async Task MigrationStatusAsync_ShouldReturnFalse_WhenThereIsZeroBedrockMigratedFileShare()
        {
            StorageEndpoint[] resultFromBedrock = Array.Empty<StorageEndpoint>();
            _helperFactoryMock.Setup(x =>
                x.GetStorageEndpointsAsync(It.IsAny<ApplicationDetails>())).ReturnsAsync(resultFromBedrock);

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

            _fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
                .ReturnsAsync(emptyResultsSet);
            bool isTenantFullyMigrated = await _sut.IsTenantFullyMigratedAsync().ConfigureAwait(false);
            isTenantFullyMigrated.Should().BeTrue();
        }

        [Test]
        public async Task MigrationStatusAsync_ShouldReturnTrue_WhenThereIsOnlyMigratedDiskInLegacyFileShare()
        {
            // ARRANGE
            var resultsSetForFileServers = new FileShareQueryResultSet
            {
                Results = new List<Result<FileShareResourceServer>>
                {
                    new Result<FileShareResourceServer>
                    {
                        Artifact = new FileShareResourceServer()
                        {
                            Name = _fileshare2Name,
                            UNCPath = _fileshare2UNC
                        }
                    }
                }
            };

            _fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
                .ReturnsAsync(resultsSetForFileServers);

            // ACT
            bool isTenantFullyMigrated = await _sut.IsTenantFullyMigratedAsync().ConfigureAwait(false);

            // ASSERT
            isTenantFullyMigrated.Should().BeTrue();
        }

        [Test]
        public async Task MigrationStatusAsync_ShouldReturnFalse_WhenException()
        {
            // ARRANGE
            _helperFactoryMock.Setup(x =>
                x.GetStorageEndpointsAsync(It.IsAny<ApplicationDetails>())).Throws(new Exception());

            // ACT
            bool isTenantFullyMigrated = await _sut.IsTenantFullyMigratedAsync().ConfigureAwait(false);

            // ASSERT
            isTenantFullyMigrated.Should().BeFalse();
        }
    }
}