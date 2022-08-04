using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Queries;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Tests.Queries
{
    [TestFixture, Category("Unit")]
    public class GetSavedSearchQueryTests
    {
        private Mock<IKeywordSearchManager> _keywordSearchManagerFake;
        private Mock<IServicesMgr> _servicesMgrFake;
        private const int _WORKSPACE_ID = 111;
        private const int _SAVED_SEACH_ID = 222;

        private GetSavedSearchQuery _sut;
        
        [SetUp]
        public void SetUp()
        {
            _keywordSearchManagerFake = new Mock<IKeywordSearchManager>();
            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IKeywordSearchManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_keywordSearchManagerFake.Object);

            _sut = new GetSavedSearchQuery(_servicesMgrFake.Object, _WORKSPACE_ID, _SAVED_SEACH_ID);
        }

        [Test]
        public void ExecuteQuery_ShouldReturnResults()
        {
            // Arrange
            const string name = "My Search";

            _keywordSearchManagerFake.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<Query>()))
                .ReturnsAsync(new KeywordSearchQueryResultSet()
                {
                    Results = new List<Result<KeywordSearch>>()
                    {
                        new Result<KeywordSearch>()
                        {
                            Artifact = new KeywordSearch()
                            {
                                ArtifactID = _SAVED_SEACH_ID,
                                Name = name
                            }
                        }
                    }
                });

            // Act
            KeywordSearchQueryResultSet results = _sut.ExecuteQuery();

            // Assert
            results.Results.Count.Should().Be(1);
            KeywordSearch artifact = results.Results.Single().Artifact;
            artifact.ArtifactID.Should().Be(_SAVED_SEACH_ID);
            artifact.Name.Should().Be(name);
        }
    }
}