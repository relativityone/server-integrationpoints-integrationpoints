using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
	public sealed class RelativitySourceCaseTagRepositoryTests
	{
		private Mock<IDestinationServiceFactoryForUser> _serviceFactory;
		private Mock<IFederatedInstance> _federatedInstance;
		private Mock<ISyncLog> _logger;
		private Mock<ITagNameFormatter> _tagNameFormatter;
		private Mock<IObjectManager> _objectManager;

		private RelativitySourceCaseTagRepository _sut;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<IDestinationServiceFactoryForUser>();
			_federatedInstance = new Mock<IFederatedInstance>();
			_logger = new Mock<ISyncLog>();
			_objectManager = new Mock<IObjectManager>();
			_tagNameFormatter = new Mock<ITagNameFormatter>();
			_tagNameFormatter.Setup(x => x.FormatWorkspaceDestinationTagName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("foo bar");
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_sut = new RelativitySourceCaseTagRepository(_serviceFactory.Object, _logger.Object);
		}

		[Test]
		public async Task ItShouldCreateDestinationWorkspaceTag()
		{
			const int tagArtifactId = 1;
			const int sourceWorkspaceArtifactId = 2;
			const int destinationWorkspaceArtifactId = 3;
			const string sourceWorkspaceName = "workspace";
			const string sourceInstanceName = "instance";
			const string sourceTagName = "Source tag name";


			CreateResult createResult = new CreateResult()
			{
				Object = new RelativityObject()
				{
					ArtifactID = tagArtifactId,
				}
			};
			_objectManager.Setup(x => x.CreateAsync(destinationWorkspaceArtifactId, It.IsAny<CreateRequest>())).ReturnsAsync(createResult);
			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(sourceInstanceName);
			var tagToCreate = new RelativitySourceCaseTag
			{
				Name = sourceTagName,
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				SourceWorkspaceName = sourceWorkspaceName,
				SourceInstanceName = sourceInstanceName
			};

			// act
			RelativitySourceCaseTag createdTag = await _sut.CreateAsync(destinationWorkspaceArtifactId, destinationWorkspaceArtifactId, tagToCreate).ConfigureAwait(false);

			// assert
			Assert.AreEqual(tagArtifactId, createdTag.ArtifactId);
			Assert.AreEqual(sourceWorkspaceName, createdTag.SourceWorkspaceName);
			Assert.AreEqual(sourceInstanceName, createdTag.SourceInstanceName);
			Assert.AreEqual(sourceWorkspaceArtifactId, createdTag.SourceWorkspaceArtifactId);
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagServiceCallFails()
		{
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, 0, new RelativitySourceCaseTag()).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagFails()
		{
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, 0, new RelativitySourceCaseTag()).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<InvalidOperationException>();
		}
	}
}