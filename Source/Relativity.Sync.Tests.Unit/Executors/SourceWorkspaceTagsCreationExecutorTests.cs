using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class SourceWorkspaceTagsCreationExecutorTests
	{
		private Mock<IDestinationWorkspaceTagRepository> _destinationWorkspaceTagRepository;
		private Mock<IDestinationWorkspaceTagsLinker> _destinationWorkspaceTagsLinker;
		private Mock<IWorkspaceNameQuery> _workspaceNameQuery;
		private Mock<IFederatedInstance> _federatedInstance;
		private Mock<IDestinationServiceFactoryForUser> _serviceFactory;

		private SourceWorkspaceTagsCreationExecutor _sut;

		[SetUp]
		public void SetUp()
		{
			_destinationWorkspaceTagRepository = new Mock<IDestinationWorkspaceTagRepository>();
			_destinationWorkspaceTagsLinker = new Mock<IDestinationWorkspaceTagsLinker>();
			_workspaceNameQuery = new Mock<IWorkspaceNameQuery>();
			_federatedInstance = new Mock<IFederatedInstance>();
			_serviceFactory = new Mock<IDestinationServiceFactoryForUser>();

			_sut = new SourceWorkspaceTagsCreationExecutor(_destinationWorkspaceTagRepository.Object, _destinationWorkspaceTagsLinker.Object,
				_workspaceNameQuery.Object, _federatedInstance.Object, _serviceFactory.Object);
		}

		[Test]
		public async Task ItShouldCreateNewDestinationWorkspaceTag()
		{
			const int sourceWorkspaceArtifactId = 1;
			const int destinationWorkspaceArtifactId = 2;
			const string destinationWorkspaceName = "destination workspace";
			Mock<ISourceWorkspaceTagsCreationConfiguration> configuration = new Mock<ISourceWorkspaceTagsCreationConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceArtifactId);
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(destinationWorkspaceArtifactId);
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactory.Object, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(destinationWorkspaceName);
			_destinationWorkspaceTagRepository.Setup(x => x.CreateAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName)).ReturnsAsync(new DestinationWorkspaceTag());
			
			// act
			await _sut.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_destinationWorkspaceTagRepository.Verify(x => x.CreateAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName));
			_destinationWorkspaceTagRepository.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<DestinationWorkspaceTag>()), Times.Never);
		}

		[Test]
		public async Task ItShouldUpdateDestinationWorkspaceTagWhenWorkspaceNameIsOutdated()
		{
			const int sourceWorkspaceArtifactId = 1;
			const int destinationWorkspaceArtifactId = 2;
			const string destinationWorkspaceNewName = "new workspace name";
			DestinationWorkspaceTag outdatedTag = new DestinationWorkspaceTag()
			{
				DestinationWorkspaceName = "old workspace name"
			};

			Mock<ISourceWorkspaceTagsCreationConfiguration> configuration = new Mock<ISourceWorkspaceTagsCreationConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceArtifactId);
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(destinationWorkspaceArtifactId);

			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(outdatedTag);
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactory.Object, destinationWorkspaceArtifactId, CancellationToken.None))
				.ReturnsAsync(destinationWorkspaceNewName);

			// act
			await _sut.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_destinationWorkspaceTagRepository.Verify(x => x.UpdateAsync(sourceWorkspaceArtifactId, It.Is<DestinationWorkspaceTag>(tag => 
				tag.DestinationWorkspaceName.Equals(destinationWorkspaceNewName, StringComparison.InvariantCulture))));
			_destinationWorkspaceTagRepository.Verify(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public async Task ItShouldUpdateDestinationWorkspaceTagWhenFederatedInstanceNameIsOutdated()
		{
			const int sourceWorkspaceArtifactId = 1;
			const int destinationWorkspaceArtifactId = 2;
			const string destinationWorkspaceName = "workspace name";
			const string federatedInstanceNewName = "new federated instance name";
			DestinationWorkspaceTag outdatedTag = new DestinationWorkspaceTag()
			{
				DestinationInstanceName = "old federated instance name"
			};

			Mock<ISourceWorkspaceTagsCreationConfiguration> configuration = new Mock<ISourceWorkspaceTagsCreationConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceArtifactId);
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(destinationWorkspaceArtifactId);

			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(outdatedTag);
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactory.Object, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(destinationWorkspaceName);
			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(federatedInstanceNewName);

			// act
			await _sut.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_destinationWorkspaceTagRepository.Verify(x => x.UpdateAsync(sourceWorkspaceArtifactId, It.Is<DestinationWorkspaceTag>(tag =>
				tag.DestinationInstanceName.Equals(federatedInstanceNewName, StringComparison.InvariantCulture))));
			_destinationWorkspaceTagRepository.Verify(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public async Task ItShouldLinkDestinationWorkspaceTagWithJobHistory()
		{
			const int destinationWorkspaceTagArtifactId = 1;
			const int sourceWorkspaceArtifactId = 2;
			const int destinationWorkspaceArtifactId = 3;
			const int jobArtifactId = 4;
			const string destinationWorkspaceName = "destination workspace";
			const string federatedInstanceName = "federated instane";
			DestinationWorkspaceTag tag = new DestinationWorkspaceTag()
			{
				ArtifactId = destinationWorkspaceTagArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
			};

			Mock<ISourceWorkspaceTagsCreationConfiguration> configuration = new Mock<ISourceWorkspaceTagsCreationConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceArtifactId);
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(destinationWorkspaceArtifactId);
			configuration.Setup(x => x.JobHistoryArtifactId).Returns(jobArtifactId);

			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactory.Object, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(destinationWorkspaceName);
			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(federatedInstanceName);
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(tag);

			// act
			await _sut.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			_destinationWorkspaceTagsLinker.Verify(x => x.LinkDestinationWorkspaceTagToJobHistoryAsync(sourceWorkspaceArtifactId, destinationWorkspaceTagArtifactId, jobArtifactId));
		}

		[Test]
		public async Task ItShouldSetDestinationWorkspaceTagArtifactIdInConfiguration()
		{
			const int destinationWorkspaceTagArtifactId = 1;
			const int sourceWorkspaceArtifactId = 2;
			const int destinationWorkspaceArtifactId = 3;
			const int jobArtifactId = 4;
			const string destinationWorkspaceName = "destination workspace";
			const string federatedInstanceName = "federated instane";
			DestinationWorkspaceTag tag = new DestinationWorkspaceTag()
			{
				ArtifactId = destinationWorkspaceTagArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
			};

			Mock<ISourceWorkspaceTagsCreationConfiguration> configuration = new Mock<ISourceWorkspaceTagsCreationConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceArtifactId);
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(destinationWorkspaceArtifactId);
			configuration.Setup(x => x.JobHistoryArtifactId).Returns(jobArtifactId);

			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactory.Object, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(destinationWorkspaceName);
			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(federatedInstanceName);
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, CancellationToken.None)).ReturnsAsync(tag);

			// act
			await _sut.ExecuteAsync(configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			configuration.Verify(x => x.SetDestinationWorkspaceTagArtifactIdAsync(destinationWorkspaceTagArtifactId));
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenWorkspaceNameQueryFails()
		{
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(It.IsAny<IDestinationServiceFactoryForUser>(), It.IsAny<int>(), CancellationToken.None)).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<ISourceWorkspaceTagsCreationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenFederatedInstanceNameReadFails()
		{
			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<ISourceWorkspaceTagsCreationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenDestinationWorkspaceTagReadFails()
		{
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None)).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<ISourceWorkspaceTagsCreationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenCreateTagFails()
		{
			DestinationWorkspaceTag tag = null;
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(tag);
			_destinationWorkspaceTagRepository.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<ISourceWorkspaceTagsCreationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenUpdateTagFails()
		{
			const string destinationWorkspaceNewName = "new workspace name";
			DestinationWorkspaceTag outdatedTag = new DestinationWorkspaceTag()
			{
				DestinationWorkspaceName = "old workspace name"
			};
			_workspaceNameQuery.Setup(x => x.GetWorkspaceNameAsync(_serviceFactory.Object, It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(destinationWorkspaceNewName);
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(outdatedTag);
			_destinationWorkspaceTagRepository.Setup(x => x.UpdateAsync(It.IsAny<int>(), outdatedTag)).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<ISourceWorkspaceTagsCreationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
		}

		[Test]
		public async Task ItShouldReturnFailedResultWhenLinkDestinationWorkspaceTagWithJobHistoryFails()
		{
			DestinationWorkspaceTag tag = new DestinationWorkspaceTag();
			_destinationWorkspaceTagRepository.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(tag);
			_destinationWorkspaceTagsLinker.Setup(x => x.LinkDestinationWorkspaceTagToJobHistoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

			// act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<ISourceWorkspaceTagsCreationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(ExecutionStatus.Failed, result.Status);
			Assert.IsNotNull(result.Exception);
			Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
		}
	}
}