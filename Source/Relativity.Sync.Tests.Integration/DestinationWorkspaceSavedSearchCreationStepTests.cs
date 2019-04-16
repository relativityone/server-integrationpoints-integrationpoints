using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.Search;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceSavedSearchCreationStepTests : FailingStepsBase<IDestinationWorkspaceSavedSearchCreationConfiguration>
	{
		private Mock<ISearchContainerManager> _searchContainerManager;
		private Mock<IKeywordSearchManager> _keywordSearchManager;
		private IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration> _executor;

		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot
			// source workspace tags, destination workspace tags
			const int expectedNumberOfExecutedSteps = 8;
			return expectedNumberOfExecutedSteps;
		}

		[SetUp]
		public void MySetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<IDestinationWorkspaceSavedSearchCreationConfiguration>(containerBuilder);

			_searchContainerManager = new Mock<ISearchContainerManager>();
			_keywordSearchManager = new Mock<IKeywordSearchManager>();
			var serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<ISearchContainerManager>()).ReturnsAsync(_searchContainerManager.Object);
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IKeywordSearchManager>()).ReturnsAsync(_keywordSearchManager.Object);

			containerBuilder.RegisterInstance(serviceFactoryMock.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterType<DestinationWorkspaceSavedSearchCreationExecutor>().As<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>();

			CorrelationId correlationId = new CorrelationId(Guid.NewGuid().ToString());

			containerBuilder.RegisterInstance(new EmptyLogger()).As<ISyncLog>();
			containerBuilder.RegisterInstance(correlationId).As<CorrelationId>();

			IContainer container = containerBuilder.Build();
			_executor = container.Resolve<IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>>();
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenSavedSearchFolderFails()
		{
			SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
			{
				Success = false
			};
			_searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<Services.Query>())).ReturnsAsync(queryResult);
			var configuration = new ConfigurationStub();

			// act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Exception.Should().BeOfType<SyncException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenSavedSearchFolderCreationFails()
		{
			SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
			{
				Success = true,
				Results = new List<Result<SearchContainer>>()
			};
			_searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<Services.Query>())).ReturnsAsync(queryResult);
			_searchContainerManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>())).Throws<InvalidOperationException>();
			var configuration = new ConfigurationStub();

			// act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Exception.Should().BeOfType<SyncException>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenCreatingSavedSearchFails()
		{
			const int folderArtifactId = 1;

			SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
			{
				Success = true,
				Results = new List<Result<SearchContainer>>()
				{
					new Result<SearchContainer>()
					{
						Artifact = new SearchContainer()
						{
							ArtifactID = folderArtifactId
						}
					}
				}
			};
			_searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<Services.Query>())).ReturnsAsync(queryResult);
			_keywordSearchManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<KeywordSearch>())).Throws<InvalidOperationException>();
			var configuration = new ConfigurationStub()
			{
				SourceJobTagName = "Some name"
			};

			// act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Exception.Should().BeOfType<SyncException>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public async Task ItShouldSetSavedSearchArtifactIdInConfiguration()
		{
			const int folderArtifactId = 1;
			const int savedSearchId = 2;

			SearchContainerQueryResultSet queryResult = new SearchContainerQueryResultSet
			{
				Success = true,
				Results = new List<Result<SearchContainer>>()
				{
					new Result<SearchContainer>()
					{
						Artifact = new SearchContainer()
						{
							ArtifactID = folderArtifactId
						}
					}
				}
			};
			_searchContainerManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<Services.Query>())).ReturnsAsync(queryResult);
			_keywordSearchManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<KeywordSearch>())).ReturnsAsync(savedSearchId);
			var configuration = new ConfigurationStub()
			{
				SourceJobTagName = "Some name"
			};

			// act
			ExecutionResult result = await _executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			configuration.SavedSearchArtifactId.Should().Be(savedSearchId);
			configuration.IsSavedSearchArtifactIdSet.Should().BeTrue();
		}
	}
}