using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions.Moq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Tests.Repositories.Implementations.CommonTests;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class JobHistoryErrorRepositoryTests : TestBase
    {
        private JobHistoryErrorRepository _sut;

        private Mock<IHelper> _helperMock;
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private int _workspaceArtifactId;

        private MassUpdateTests _massUpdateTests;

        [SetUp]
        public override void SetUp()
        {
            _helperMock = new Mock<IHelper>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();
            _workspaceArtifactId = 456;
            var objectManagerFactory = new Mock<IRelativityObjectManagerFactory>();
            objectManagerFactory
                .Setup(x => x.CreateRelativityObjectManager(It.IsAny<int>()))
                .Returns(_objectManagerMock.Object);

            _sut = new JobHistoryErrorRepository(
                _helperMock.Object,
                objectManagerFactory.Object,
                _workspaceArtifactId);

            _massUpdateTests = new MassUpdateTests(_sut, _objectManagerMock);
        }

        #region Read
        private static IEnumerable<int>[] ReadSource = {
            new List<int>(),
            new List<int> { 1 },
            new List<int> { 1 , 2, 3 , 4 }
        };

        [Test, TestCaseSource(nameof(ReadSource))]
        public void Read(IEnumerable<int> artifactIds)
        {
            // act
            _sut.Read(artifactIds);

            // assert
            _objectManagerMock.Verify(
                x => x.Query<JobHistoryError>(
                    It.Is<QueryRequest>(z => z.Condition == $"'{ArtifactQueryFieldNames.ArtifactID}' in [{string.Join(",", artifactIds)}]"),
                    ExecutionIdentity.CurrentUser
                    )
                );
        }
        #endregion

        #region DeleteItemLevelErrorsSavedSearch
        private static List<Task>[] DeleteItemLevelErrorsSavedSearch = {
            new List<Task> { CreateCompletedTask() },
            new List<Task> { CreateErrorTask(), CreateCompletedTask()},
            new List<Task> { CreateErrorTask(), CreateErrorTask(), CreateErrorTask() },
            new List<Task> { CreateErrorTask(), CreateErrorTask(), CreateCompletedTask() },
        };

        private static Task CreateCompletedTask()
        {
            Task task = new Task(() => { });
            task.Start();
            task.Wait();
            return task;
        }

        private static Task CreateErrorTask()
        {
            Task task = null;
            try
            {
                task = new Task(() => { throw new Exception(); });
                task.Start();
                task.Wait();
            }
            catch (Exception)
            { }
            return task;
        }

        [Test, TestCaseSource(nameof(DeleteItemLevelErrorsSavedSearch))]
        public void DeleteItemLevelErrorsSavedSearch_GoldFlow(List<Task> mockTasks)
        {
            // arrange
            Mock<IKeywordSearchManager> keywordSearchMock = new Mock<IKeywordSearchManager>();

            int searchId = 123456;
            _helperMock
                .Setup(
                    x => x
                        .GetServicesManager()
                        .CreateProxy<IKeywordSearchManager>(It.IsAny<ExecutionIdentity>())
                    )
                .Returns(keywordSearchMock.Object);
            if (mockTasks.Count < 2)
            {
                keywordSearchMock
                    .Setup(x => x.DeleteSingleAsync(_workspaceArtifactId, searchId))
                    .Returns(mockTasks[0]);
            }
            else
            {
                keywordSearchMock
                    .SetupSequence(x => x.DeleteSingleAsync(_workspaceArtifactId, searchId))
                    .Returns(mockTasks);
            }

            // act
            _sut.DeleteItemLevelErrorsSavedSearch(searchId);

            // assert
            keywordSearchMock.Verify(
                x => x.DeleteSingleAsync(_workspaceArtifactId, searchId),
                Times.Exactly(mockTasks.Count));
        }

        #endregion

        [Test]
        public Task MassUpdateAsync_ShouldBuildProperRequest()
        {
            return _massUpdateTests.ShouldBuildProperRequest();
        }

        [TestCase(true)]
        [TestCase(false)]
        public Task MassUpdateAsync_ShouldReturnCorrectResult(bool expectedResult)
        {
            return _massUpdateTests.ShouldReturnCorrectResult(expectedResult);
        }

        [Test]
        public void MassUpdateAsync_ShouldRethrowObjectManagerException()
        {
            _massUpdateTests.ShouldRethrowObjectManagerException();
        }
    }
}
