using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;

namespace Relativity.Sync.Tests.Unit.SyncConfiguration
{
    [TestFixture]
    public class SyncConfigurationBuilderTests
    {
        private readonly int _sourceWorkspaceId = 1;
        private readonly int _destinationWorkspaceId = 2;
        private readonly int _jobHistoryId = 3;

        private SyncContext _syncContext;
        private Mock<IServicesMgr> _servicesManager;
        private SyncConfigurationBuilder _sut;
        private Mock<IArtifactGuidManager> _guidManagerMock;
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<IFieldManager> _fieldManagerMock;

        [SetUp]
        public void SetUp()
        {
            _syncContext = new SyncContext(_sourceWorkspaceId, _destinationWorkspaceId, _jobHistoryId);
            _servicesManager = new Mock<IServicesMgr>();
            _guidManagerMock = new Mock<IArtifactGuidManager>();
            _objectManagerMock = new Mock<IObjectManager>();
            _fieldManagerMock = new Mock<IFieldManager>();

            _guidManagerMock.Setup(x => x.ReadMultipleArtifactIdsAsync(_sourceWorkspaceId, It.IsAny<List<Guid>>()))
                .ReturnsAsync((int _, List<Guid> guids) =>
                {
                    return guids.Select(g => new GuidArtifactIDPair() { Guid = g, ArtifactID = 5 })
                        .ToList();
                });

            _objectManagerMock.Setup(x =>
                    x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new QueryResult()
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject()
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

            _objectManagerMock.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<ReadRequest>()))
                .ReturnsAsync(new ReadResult()
                {
                    ObjectType = new ObjectType()
                });

            _objectManagerMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>()))
                .ReturnsAsync(new CreateResult()
                {
                    Object = new RelativityObject() { ArtifactID = 1 }
                });

            _servicesManager.Setup(x => x.CreateProxy<IArtifactGuidManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_guidManagerMock.Object);

            _servicesManager.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_objectManagerMock.Object);

            _servicesManager.Setup(x => x.CreateProxy<IFieldManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_fieldManagerMock.Object);

            _servicesManager.Setup(x => x.CreateProxy<IArtifactGuidManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_guidManagerMock.Object);

            _servicesManager.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_objectManagerMock.Object);

            _servicesManager.Setup(x => x.CreateProxy<IFieldManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_fieldManagerMock.Object);

            _sut = new SyncConfigurationBuilder(_syncContext, _servicesManager.Object, new EmptyLogger());
        }

        [Test]
        [TestCaseSource(nameof(RdoOptionsMembers))]
        public void ConfigureRdo_ShouldCheckAllMembersForEmptyGuids(PropertyInfo rdoPath, PropertyInfo propertyPath)
        {
            // Arrange
            var rdoOptions = DefaultGuids.DefaultRdoOptions;

            var rdo = rdoPath.GetValue(rdoOptions);
            propertyPath.SetValue(rdo, Guid.Empty);

            Action action = () => _sut.ConfigureRdos(rdoOptions);

            // Act & Assert
            action.Should().ThrowExactly<InvalidSyncConfigurationException>()
                .WithMessage(
                    $"GUID value for {rdoPath.PropertyType.Name}.{propertyPath.Name} is invalid: {Guid.Empty.ToString()}");
        }

        [Test]
        public async Task SaveAsync_ShouldQueryAllRdoGuids()
        {
            // Arrange
            var rdoOptions = DefaultGuids.DefaultRdoOptions;

            // Act
            await _sut.ConfigureRdos(rdoOptions)
                .ConfigureDocumentSync(new DocumentSyncOptions(0, 0))
                .SaveAsync()
                .ConfigureAwait(false);

            // Assert
            var expectedQueriedGuids = RdoOptionsMembers().Select(x =>
            {
                var rdoPath = x.Arguments[0] as PropertyInfo;
                var propertyPath = x.Arguments[1] as PropertyInfo;

                var rdo = rdoPath.GetValue(rdoOptions);
                return (Guid)propertyPath.GetValue(rdo);
            }).ToArray();

            expectedQueriedGuids.ForEach(g => _guidManagerMock.Verify(x =>
                x.ReadMultipleArtifactIdsAsync(_sourceWorkspaceId, It.Is<List<Guid>>(l => l.Contains(g)))));
        }

        [Test]
        [TestCaseSource(nameof(RdoOptionsMembers))]
        public void SaveAsync_ShouldThrow_WhenRdoGuidDoeNotExist(PropertyInfo rdoPath, PropertyInfo propertyPath)
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            object rdo = rdoPath.GetValue(rdoOptions);
            Guid notExistingGuid = (Guid)propertyPath.GetValue(rdo);

            _guidManagerMock.Setup(x => x.ReadMultipleArtifactIdsAsync(_sourceWorkspaceId, It.IsAny<List<Guid>>()))
                .ReturnsAsync((int _, List<Guid> guids) =>
                {
                    return guids.Except(new[] { notExistingGuid })
                        .Select(g => new GuidArtifactIDPair() { Guid = g, ArtifactID = 5 })
                        .ToList();
                });

            // Act && Assert
            Func<Task> action = async () =>
            {
                await _sut.ConfigureRdos(rdoOptions)
                    .ConfigureDocumentSync(new DocumentSyncOptions(0, 0))
                    .SaveAsync()
                    .ConfigureAwait(false);
            };

            action.Should().Throw<InvalidSyncConfigurationException>().WithMessage(
                $"Guid {notExistingGuid} for {rdoPath.PropertyType.Name}.{propertyPath.Name} does not exist");
        }

        [Test]
        public void SyncConfigurationRootBuilderBase_ShouldCopyOverRdoValues()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            // Act
            SyncConfigurationRootBuilderBase builder = (SyncConfigurationRootBuilderBase)_sut.ConfigureRdos(rdoOptions)
                .ConfigureDocumentSync(new DocumentSyncOptions(0, 0));

            // Assert
            builder.SyncConfiguration.JobHistoryCompletedItemsField.Should()
                .Be(rdoOptions.JobHistory.CompletedItemsCountGuid);

            builder.SyncConfiguration.JobHistoryDestinationWorkspaceInformationField.Should()
                .Be(rdoOptions.JobHistory.DestinationWorkspaceInformationGuid);

            builder.SyncConfiguration.JobHistoryGuidFailedField.Should()
                .Be(rdoOptions.JobHistory.FailedItemsCountGuid);

            builder.SyncConfiguration.JobHistoryType.Should()
                .Be(rdoOptions.JobHistory.JobHistoryTypeGuid);

            builder.SyncConfiguration.JobHistoryGuidTotalField.Should()
                .Be(rdoOptions.JobHistory.TotalItemsCountGuid);

            builder.SyncConfiguration.JobHistoryErrorErrorMessages.Should()
                .Be(rdoOptions.JobHistoryError.ErrorMessageGuid);

            builder.SyncConfiguration.JobHistoryErrorErrorStatus.Should()
                .Be(rdoOptions.JobHistoryError.ErrorStatusGuid);

            builder.SyncConfiguration.JobHistoryErrorErrorType.Should()
                .Be(rdoOptions.JobHistoryError.ErrorTypeGuid);

            builder.SyncConfiguration.JobHistoryErrorItemLevelError.Should()
                .Be(rdoOptions.JobHistoryError.ItemLevelErrorChoiceGuid);

            builder.SyncConfiguration.JobHistoryErrorJobLevelError.Should()
                .Be(rdoOptions.JobHistoryError.JobLevelErrorChoiceGuid);

            builder.SyncConfiguration.JobHistoryErrorName.Should()
                .Be(rdoOptions.JobHistoryError.NameGuid);

            builder.SyncConfiguration.JobHistoryErrorSourceUniqueId.Should()
                .Be(rdoOptions.JobHistoryError.SourceUniqueIdGuid);

            builder.SyncConfiguration.JobHistoryErrorStackTrace.Should()
                .Be(rdoOptions.JobHistoryError.StackTraceGuid);

            builder.SyncConfiguration.JobHistoryErrorTimeStamp.Should()
                .Be(rdoOptions.JobHistoryError.TimeStampGuid);

            builder.SyncConfiguration.JobHistoryErrorType.Should()
                .Be(rdoOptions.JobHistoryError.TypeGuid);

            builder.SyncConfiguration.JobHistoryErrorNewChoice.Should()
                .Be(rdoOptions.JobHistoryError.NewStatusGuid);

            builder.SyncConfiguration.JobHistoryErrorJobHistoryRelation.Should()
                .Be(rdoOptions.JobHistoryError.JobHistoryRelationGuid);
        }

        [Test]
        public void SyncConfigurationRootBuilderBase_ShouldSetRdoTypeToDocument()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            // Act
            SyncConfigurationRootBuilderBase builder = (SyncConfigurationRootBuilderBase)_sut.ConfigureRdos(rdoOptions)
                .ConfigureDocumentSync(new DocumentSyncOptions(0, 0));

            // Assert
            builder.SyncConfiguration.RdoArtifactTypeId.Should().Be((int)ArtifactType.Document);
            builder.SyncConfiguration.DestinationRdoArtifactTypeId.Should().Be((int)ArtifactType.Document);
        }

        [Test]
        public void SyncConfigurationRootBuilderBase_ShouldSetRdoTypeToDocument_ForImageFlow()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            // Act
            SyncConfigurationRootBuilderBase builder = (SyncConfigurationRootBuilderBase)_sut.ConfigureRdos(rdoOptions)
                .ConfigureImageSync(new ImageSyncOptions(DataSourceType.Production, 0, DestinationLocationType.Folder, 0));

            // Assert
            builder.SyncConfiguration.RdoArtifactTypeId.Should().Be((int)ArtifactType.Document);
            builder.SyncConfiguration.DestinationRdoArtifactTypeId.Should().Be((int)ArtifactType.Document);
        }

        [Test]
        public void SyncConfigurationRootBuilderBase_ShouldSetRdoType_ForNonDocumentFlow()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;
            const int rdoType = 69;
            const int destinationRdoType = 420;

            // Act
            SyncConfigurationRootBuilderBase builder = (SyncConfigurationRootBuilderBase)_sut.ConfigureRdos(rdoOptions)
                .ConfigureNonDocumentSync(new NonDocumentSyncOptions(0, rdoType, destinationRdoType));

            // Assert
            builder.SyncConfiguration.RdoArtifactTypeId.Should().Be(rdoType);
            builder.SyncConfiguration.DestinationRdoArtifactTypeId.Should().Be(destinationRdoType);
        }

        [Test]
        public void SyncConfigurationBuilderBase_ShouldSetLogItemLevelErrors()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            // Act
            IDocumentSyncConfigurationBuilder sut = new SyncConfigurationBuilder(_syncContext, _servicesManager.Object, new EmptyLogger())
                .ConfigureRdos(rdoOptions)
                .ConfigureDocumentSync(new DocumentSyncOptions(1, 1));

            // Assert
            (sut as SyncConfigurationRootBuilderBase).SyncConfiguration.LogItemLevelErrors.Should()
                .BeTrue();
        }

        [Test]
        public void SyncConfigurationBuilderBase_DisableItemLevelErrorLogging_ShouldSetLogItemLevelErrorsToFalse()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            // Act
            IDocumentSyncConfigurationBuilder sut = new SyncConfigurationBuilder(_syncContext, _servicesManager.Object, new EmptyLogger())
                .ConfigureRdos(rdoOptions)
                .ConfigureDocumentSync(new DocumentSyncOptions(1, 1))
                .DisableItemLevelErrorLogging();

            // Assert
            (sut as SyncConfigurationRootBuilderBase).SyncConfiguration.LogItemLevelErrors.Should()
                .BeFalse();
        }

        [Test]
        public void Validate_ShouldThrowException_WhenDocumentSavedSearchWithoutTaggingIsConfigured()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            IDocumentSyncConfigurationBuilder sut = new SyncConfigurationBuilder(_syncContext, _servicesManager.Object, new EmptyLogger())
                .ConfigureRdos(rdoOptions)
                .ConfigureDocumentSync(new DocumentSyncOptions(1, 1)
                {
                    EnableTagging = false
                })
                .CreateSavedSearch(new CreateSavedSearchOptions(true));

            // Act
            Func<Task> action = () => sut.SaveAsync();

            // Assert
            action.Should().ThrowAsync<InvalidSyncConfigurationException>();
        }

        [Test]
        public void Validate_ShouldThrowException_WhenImageSavedSearchWithoutTaggingIsConfigured()
        {
            // Arrange
            RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;

            IImageSyncConfigurationBuilder sut = new SyncConfigurationBuilder(_syncContext, _servicesManager.Object, new EmptyLogger())
                .ConfigureRdos(rdoOptions)
                .ConfigureImageSync(
                    new ImageSyncOptions(
                        DataSourceType.SavedSearch,
                        1,
                        DestinationLocationType.Folder,
                        2))
                .CreateSavedSearch(new CreateSavedSearchOptions(true));

            // Act
            Func<Task> action = () => sut.SaveAsync();

            // Assert
            action.Should().ThrowAsync<InvalidSyncConfigurationException>();
        }

		[Test]
		public void SyncConfigurationRootBuilderBase_ExecutingApplicationVersion_LatestVersioning()
		{
			// Arrange
			RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;
			const int rdoType = 69;
			const int destinationRdoType = 420;
            string expectedExecutingApplicationVersion = "23013.2.2.";
			Version version = new Version(23013, 2, 2, 250);
		    SyncContext syncContext = new SyncContext(_sourceWorkspaceId, _destinationWorkspaceId, _jobHistoryId, string.Empty, version);
			
            // Act
			SyncConfigurationRootBuilderBase builder = (SyncConfigurationRootBuilderBase)_sut.ConfigureRdos(rdoOptions, syncContext)
				.ConfigureNonDocumentSync(new NonDocumentSyncOptions(0, rdoType, destinationRdoType));

			// Assert
			builder.SyncConfiguration.ExecutingApplicationVersion.Should().Be(expectedExecutingApplicationVersion);
		}

		[Test]
		public void SyncConfigurationRootBuilderBase_ExecutingApplicationVersion_PreviousVersioning()
		{
			// Arrange
			RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;
			const int rdoType = 69;
			const int destinationRdoType = 420;
			string expectedExecutingApplicationVersion = "13.2.2.250";
			Version version = new Version(13, 2, 2, 250);
			SyncContext syncContext = new SyncContext(_sourceWorkspaceId, _destinationWorkspaceId, _jobHistoryId, string.Empty, version);

			// Act
			SyncConfigurationRootBuilderBase builder = (SyncConfigurationRootBuilderBase)_sut.ConfigureRdos(rdoOptions, syncContext)
				.ConfigureNonDocumentSync(new NonDocumentSyncOptions(0, rdoType, destinationRdoType));

			// Assert
			builder.SyncConfiguration.ExecutingApplicationVersion.Should().Be(expectedExecutingApplicationVersion);
		}

		[Test]
		public void SyncConfigurationRootBuilderBase_ExecutingApplicationVersion_MajorMinorVersions()
		{
			// Arrange
			RdoOptions rdoOptions = DefaultGuids.DefaultRdoOptions;
			const int rdoType = 69;
			const int destinationRdoType = 420;
			string expectedExecutingApplicationVersion = "13.2";
			Version version = new Version(13, 2);
			SyncContext syncContext = new SyncContext(_sourceWorkspaceId, _destinationWorkspaceId, _jobHistoryId, string.Empty, version);

			// Act
			SyncConfigurationRootBuilderBase builder = (SyncConfigurationRootBuilderBase)_sut.ConfigureRdos(rdoOptions, syncContext)
				.ConfigureNonDocumentSync(new NonDocumentSyncOptions(0, rdoType, destinationRdoType));

			// Assert
			builder.SyncConfiguration.ExecutingApplicationVersion.Should().Be(expectedExecutingApplicationVersion);
		}

		private static IEnumerable<TestCaseData> RdoOptionsMembers()
        {
            var properties = typeof(RdoOptions).GetProperties()
                .SelectMany(x =>
                    x.PropertyType.GetProperties().Select(p => new { Rdo = x, Property = p }));

            return properties
                .Select(x =>
                {
                    var testCase = new TestCaseData(x.Rdo, x.Property);
                    testCase.TestName = $"{x.Rdo.Name}.{x.Property.Name}";

                    return testCase;
                })
                .ToArray();
        }

		
	}
}
