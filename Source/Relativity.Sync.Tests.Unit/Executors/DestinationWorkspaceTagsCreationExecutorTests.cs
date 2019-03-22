using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class DestinationWorkspaceTagsCreationExecutorTests
	{
		private Mock<ISourceCaseTagService> _sourceCaseTagService;
		private Mock<ISourceJobTagService> _sourceJobTagService;

		private DestinationWorkspaceTagsCreationExecutor _sut;

		[SetUp]
		public void SetUp()
		{
			_sourceJobTagService = new Mock<ISourceJobTagService>();
			_sourceCaseTagService = new Mock<ISourceCaseTagService>();

			_sut = new DestinationWorkspaceTagsCreationExecutor(_sourceCaseTagService.Object, _sourceJobTagService.Object);
		}

		[Test]
		public async Task ItShouldSetSourceWorkspaceTag()
		{
			const int sourceCaseArtifactId = 1;
			const string sourceCaseTagName = "tag name";
			RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag()
			{
				ArtifactId = sourceCaseArtifactId,
				Name = sourceCaseTagName
			};
			_sourceCaseTagService.Setup(x => x.CreateOrUpdateSourceCaseTagAsync(It.IsAny<IDestinationWorkspaceTagsCreationConfiguration>(), CancellationToken.None)).ReturnsAsync(sourceCaseTag);
			_sourceJobTagService.Setup(x => x.CreateSourceJobTagAsync(It.IsAny<IDestinationWorkspaceTagsCreationConfiguration>(), It.IsAny<int>(), CancellationToken.None))
				.ReturnsAsync(new RelativitySourceJobTag());

			Mock<IDestinationWorkspaceTagsCreationConfiguration> configuration = new Mock<IDestinationWorkspaceTagsCreationConfiguration>();

			// act
			await _sut.ExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			configuration.Verify(x => x.SetSourceWorkspaceTag(It.Is<int>(i => i == sourceCaseArtifactId), It.Is<string>(s => s.Equals(sourceCaseTagName, StringComparison.InvariantCulture))));
		}

		[Test]
		public async Task ItShouldSetSourceJobTag()
		{
			const int sourceJobTagArtifactId = 1;
			const string sourceJobTagName = "tag name";
			RelativitySourceJobTag sourceJobTag = new RelativitySourceJobTag()
			{
				ArtifactId = sourceJobTagArtifactId,
				Name = sourceJobTagName
			};
			_sourceCaseTagService.Setup(x => x.CreateOrUpdateSourceCaseTagAsync(It.IsAny<IDestinationWorkspaceTagsCreationConfiguration>(), CancellationToken.None)).ReturnsAsync(new RelativitySourceCaseTag());
			_sourceJobTagService.Setup(x => x.CreateSourceJobTagAsync(It.IsAny<IDestinationWorkspaceTagsCreationConfiguration>(), It.IsAny<int>(), CancellationToken.None))
				.ReturnsAsync(sourceJobTag);

			Mock<IDestinationWorkspaceTagsCreationConfiguration> configuration = new Mock<IDestinationWorkspaceTagsCreationConfiguration>();

			// act
			await _sut.ExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			configuration.Verify(x => x.SetSourceJobTag(It.Is<int>(i => i == sourceJobTagArtifactId), It.Is<string>(s => s.Equals(sourceJobTagName, StringComparison.InvariantCulture))));
		}
	}
}