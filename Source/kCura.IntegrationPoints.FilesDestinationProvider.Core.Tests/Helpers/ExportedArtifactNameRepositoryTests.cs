using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;
using Relativity.Services.View;
using Action = System.Action;
using Query = Relativity.Services.Query;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Helpers
{
    public class ExportedArtifactNameRepositoryTests
    {
        private Mock<IKeywordSearchManager> _keywordSearchManagerFake;
        private Mock<IViewManager> _viewManager;
        private Mock<IServicesMgr> _servicesMgrFake;

        private const int _WORKSPACE_ID = 111;
        private const int _SAVED_SEACH_ID = 222;

        private ExportedArtifactNameRepository _sut;

        [SetUp]
        public void SetUp()
        {
            _servicesMgrFake = new Mock<IServicesMgr>();

            _keywordSearchManagerFake = new Mock<IKeywordSearchManager>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IKeywordSearchManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_keywordSearchManagerFake.Object);

            _viewManager = new Mock<IViewManager>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IViewManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_viewManager.Object);

            _sut = new ExportedArtifactNameRepository(_servicesMgrFake.Object);
        }

        [Test]
        public void GetViewName_ShouldReturnViewName()
        {
            // Arrange
            const int viewId = 333;
            const string viewName = "My View";

            _viewManager.Setup(x => x.ReadSingleAsync(_WORKSPACE_ID, viewId)).ReturnsAsync(new View()
            {
                ArtifactID = viewId,
                Name = viewName
            });

            // Act
            string actualViewName = _sut.GetViewName(_WORKSPACE_ID, viewId);

            // Assert
            actualViewName.Should().Be(viewName);
        }

        [Test]
        public void GetSavedSearchName_ShouldReturnSavedSearchName()
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
                    },
                    Success = true
                });

            // Act
            string actualName = _sut.GetSavedSearchName(_WORKSPACE_ID, _SAVED_SEACH_ID);

            // Assert
            actualName.Should().Be(name);
        }

        [Test]
        public void GetSavedSearchName_ShouldThrow_WhenKeplerDoesNotReturnSuccess()
        {
            // Arrange
            _keywordSearchManagerFake.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<Query>()))
                .ReturnsAsync(new KeywordSearchQueryResultSet()
                {
                    Success = false
                });

            // Act
            Action action = () => _sut.GetSavedSearchName(_WORKSPACE_ID, _SAVED_SEACH_ID);

            // Assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void GetSavedSearchName_ShouldThrow_WhenSavedSearchNotFound()
        {
            // Arrange
            _keywordSearchManagerFake.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<Query>()))
                .ReturnsAsync(new KeywordSearchQueryResultSet()
                {
                    Results = new List<Result<KeywordSearch>>(),
                    Success = true
                });

            // Act
            Action action = () => _sut.GetSavedSearchName(_WORKSPACE_ID, _SAVED_SEACH_ID);

            // Assert
            action.ShouldThrow<IntegrationPointsException>();
        }
    }
}
