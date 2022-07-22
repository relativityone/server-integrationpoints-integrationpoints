using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Utils;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal class JobHistoryErrorRepositoryTests : SystemTest
    {
        private WorkspaceRef _workspace;
        private ISourceServiceFactoryForUser _serviceFactoryForUser;
        private IDateTime _dateTime;
        private IAPILog _logger;

        private readonly Guid _jobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
        private readonly Guid _errorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
        private readonly Guid _errorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");
        private readonly Guid _errorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");
        private readonly Guid _sourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
        private readonly Guid _stackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");

        [SetUp]
        public async Task SetUp()
        {
            Mock<IRandom> randomFake = new Mock<IRandom>();
            Mock<IAPILog> syncLogMock = new Mock<IAPILog>();

            _workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _serviceFactoryForUser = new ServiceFactoryForUser(ServiceFactory, new DynamicProxyFactoryStub(), 
                randomFake.Object, syncLogMock.Object);
            _dateTime = new DateTimeWrapper();
            _logger = new EmptyLogger();
        }

        [IdentifiedTest("125edbf4-7c69-4be4-8b7d-535571e75abe")]
        public async Task ItShouldCreateJobHistoryError()
        {
            // Arrange
            int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspace.ArtifactID, "Totally unique job history name").ConfigureAwait(false);
            ErrorType expectedErrorType = ErrorType.Item;
            ErrorStatus expErrorStatus = ErrorStatus.New;
            string expectedErrorMessage = "Mayday, mayday";
            string expectedSourceUniqueId = "Totally unique Id";
            string expectedStackTrace = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

            CreateJobHistoryErrorDto createDto = new CreateJobHistoryErrorDto(expectedErrorType)
            {
                ErrorMessage = expectedErrorMessage,
                SourceUniqueId = expectedSourceUniqueId,
                StackTrace = expectedStackTrace,
            };

            var configurationStub = new ConfigurationStub();
            JobHistoryErrorRepository instance = new JobHistoryErrorRepository(_serviceFactoryForUser, configurationStub, configurationStub,
                _dateTime, _logger, new WrapperForRandom());

            // Act
            int createdErrorArtifactId = await instance.CreateAsync(_workspace.ArtifactID, expectedJobHistoryArtifactId, createDto).ConfigureAwait(false);

            // Assert
            createdErrorArtifactId.Should().NotBe(0);

            RelativityObject error = await QueryForCreatedJobHistoryError(createdErrorArtifactId).ConfigureAwait(false);
            error[_errorMessageField].Value.Should().Be(expectedErrorMessage);
            error[_stackTraceField].Value.Should().Be(expectedStackTrace);
            error[_sourceUniqueIdField].Value.Should().Be(expectedSourceUniqueId);
            error[_errorStatusField].Value.As<Choice>().Name.Should().Be(expErrorStatus.ToString());
            error[_errorTypeField].Value.As<Choice>().Name.Should().Be(expectedErrorType.ToString());
            error.ParentObject.ArtifactID.Should().Be(expectedJobHistoryArtifactId);
        }

        [IdentifiedTest("737460ff-47c9-4cf3-905a-17439a5e1acb")]
        public async Task ItemLevelErrorMassCreation_ShouldHandleAllErrors_WhenRequestEntityIsToLarge()
        {
            // Arrange
            int jobHistoryArtifactID = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspace.ArtifactID).ConfigureAwait(false);
            const int errorMsgSize = 10;
            const int stackTraceSize = errorMsgSize * 10;
            const int itemLevelErrorsCount = 20000;

            CreateJobHistoryErrorDto itemLevelError = new CreateJobHistoryErrorDto(ErrorType.Item)
            {
                ErrorMessage = GetLongTextString(errorMsgSize),
                SourceUniqueId = Guid.NewGuid().ToString(),
                StackTrace = GetLongTextString(stackTraceSize)
            };

            IList<CreateJobHistoryErrorDto> itemLevelErrors = Enumerable.Repeat(itemLevelError, itemLevelErrorsCount).ToList();

            var configurationStub = new ConfigurationStub();
            JobHistoryErrorRepository sut = new JobHistoryErrorRepository(_serviceFactoryForUser, configurationStub, configurationStub,
                _dateTime, _logger, new WrapperForRandom());

            // Act
            IEnumerable<int> result = await sut.MassCreateAsync(_workspace.ArtifactID, jobHistoryArtifactID, itemLevelErrors).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(itemLevelErrorsCount);
        }

        private static string GetLongTextString(int count) => new string('.', count);

        private async Task<RelativityObject> QueryForCreatedJobHistoryError(int jobHistoryErrorArtifactId)
        {
            IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>();

            QueryRequest queryRequest = new QueryRequest
            {
                Condition = $"\"ArtifactId\"=={jobHistoryErrorArtifactId}",
                ObjectType = new ObjectTypeRef {Guid = _jobHistoryErrorObject},
                IncludeNameInQueryResult = true,
                Fields = new List<FieldRef>
                {
                    new FieldRef{Guid = _sourceUniqueIdField},
                    new FieldRef{Guid = _errorMessageField},
                    new FieldRef{Guid = _errorTypeField},
                    new FieldRef{Guid = _stackTraceField},
                    new FieldRef{Guid = _errorStatusField},
                }
            };

            QueryResult queryResult = await objectManager.QueryAsync(_workspace.ArtifactID, queryRequest, 1, 1).ConfigureAwait(false);
            return queryResult.Objects.First();
        }
    }
}
