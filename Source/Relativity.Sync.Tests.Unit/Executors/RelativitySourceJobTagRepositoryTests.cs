using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class RelativitySourceJobTagRepositoryTests
	{
		private Mock<IObjectManager> _objectManager;

		private RelativitySourceJobTagRepository _sut;

		[SetUp]
		public void SetUp()
		{
			Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_objectManager = new Mock<IObjectManager>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_sut = new RelativitySourceJobTagRepository(serviceFactory.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldCreateDestinationWorkspaceTag()
		{
			const int destinationWorkspaceArtifactId = 5;

			const int tagArtifactId = 1;
			const int artifactTypeId = 2;
			const int jobHistoryArtifactId = 3;
			const string sourceJobName = "source job name";
			const int sourceCaseTagArtifactId = 4;
			const string sourceJobTagName = "source job tag name";

			QueryResult jobHistoryNameQueryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						Name = sourceJobName
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(jobHistoryNameQueryResult);

			CreateResult createResult = new CreateResult()
			{
				Object = new RelativityObject()
				{
					ArtifactID = tagArtifactId,
				}
			};

			_objectManager.Setup(x => x.CreateAsync(destinationWorkspaceArtifactId, It.IsAny<CreateRequest>())).ReturnsAsync(createResult);

			RelativitySourceJobTag tagToCreate = new RelativitySourceJobTag
			{
				Name = sourceJobTagName,
				ArtifactTypeId = artifactTypeId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = sourceJobName,
				SourceCaseTagArtifactId = sourceCaseTagArtifactId
			};

			// act
			RelativitySourceJobTag createdTag = await _sut.CreateAsync(destinationWorkspaceArtifactId, tagToCreate, CancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(tagArtifactId, createdTag.ArtifactId);
			Assert.AreEqual(artifactTypeId, createdTag.ArtifactTypeId);
			Assert.AreEqual(jobHistoryArtifactId, createdTag.JobHistoryArtifactId);
			Assert.AreEqual(sourceJobName, createdTag.JobHistoryName);
			Assert.AreEqual(sourceJobTagName, createdTag.Name);
			Assert.AreEqual(sourceCaseTagArtifactId, createdTag.SourceCaseTagArtifactId);
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagServiceCallFails()
		{
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(new QueryResult());
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, new RelativitySourceJobTag(), CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<RelativitySourceJobTagRepositoryException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagFails()
		{
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()))
				.ReturnsAsync(new QueryResult());
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, new RelativitySourceJobTag(), CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<RelativitySourceJobTagRepositoryException>().WithInnerException<InvalidOperationException>();
		}
	}
}