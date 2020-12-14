#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.Relativity.Client;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;
using Action = System.Action;
using Query = Relativity.Services.Query;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Helpers
{
	public class ExportedArtifactNameRepositoryTests
	{
		private Mock<IKeywordSearchManager> _keywordSearchManagerFake;
		private Mock<IServicesMgr> _servicesMgrFake;
		private Mock<IRSAPIClient> _rsapiFake;
		private Mock<IServiceManagerProvider> _serviceManagerProvider;

		private const int _WORKSPACE_ID = 111;
		private const int _SAVED_SEACH_ID = 222;

		private ExportedArtifactNameRepository _sut;

		[SetUp]
		public void SetUp()
		{
			_keywordSearchManagerFake = new Mock<IKeywordSearchManager>();
			_servicesMgrFake = new Mock<IServicesMgr>();
			_servicesMgrFake.Setup(x => x.CreateProxy<IKeywordSearchManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_keywordSearchManagerFake.Object);

			_serviceManagerProvider = new Mock<IServiceManagerProvider>();
			_rsapiFake = new Mock<IRSAPIClient>();

			_sut = new ExportedArtifactNameRepository(_servicesMgrFake.Object, _rsapiFake.Object, _serviceManagerProvider.Object);
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
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
