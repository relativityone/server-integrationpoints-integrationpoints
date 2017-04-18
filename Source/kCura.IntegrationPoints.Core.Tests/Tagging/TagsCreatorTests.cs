using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
	public class TagsCreatorTests : TestBase
	{
		private TagsCreator _instance;

		private ISourceJobManager _sourceJobManager;
		private ISourceWorkspaceManager _sourceWorkspaceManager;

		public override void SetUp()
		{
			_sourceJobManager = Substitute.For<ISourceJobManager>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();

			var helper = Substitute.For<IHelper>();

			_instance = new TagsCreator(_sourceJobManager, _sourceWorkspaceManager, helper);
		}

		public void ItShouldCreateTags()
		{
			int sourceWorkspaceArtifactId = 843740;
			int destinationWorkspaceArtifactId = 527695;
			int jobHistoryArtifactId = 847715;
			int? federatedInstanceArtifactId = 561710;

			var sourceWorkspaceDto = new SourceWorkspaceDTO
			{
				ArtifactTypeId = 10,
				ArtifactId = 831219
			};
			var sourceJobDto = new SourceJobDTO();

			_sourceWorkspaceManager
				.InitializeWorkspace(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, federatedInstanceArtifactId)
				.Returns(sourceWorkspaceDto);

			_sourceJobManager
				.InitializeWorkspace(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, sourceWorkspaceDto.ArtifactTypeId, sourceWorkspaceDto.ArtifactId, jobHistoryArtifactId)
				.Returns(sourceJobDto);

			// ACT
			var tagsContainer = _instance.CreateTags(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, jobHistoryArtifactId, federatedInstanceArtifactId);

			// ASSERT
			Assert.That(tagsContainer.SourceJobDto, Is.EqualTo(sourceJobDto));
			Assert.That(tagsContainer.SourceWorkspaceDto, Is.EqualTo(sourceWorkspaceDto));

			_sourceWorkspaceManager
				.Received(1)
				.InitializeWorkspace(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, federatedInstanceArtifactId);

			_sourceJobManager
				.Received(1)
				.InitializeWorkspace(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, sourceWorkspaceDto.ArtifactTypeId, sourceWorkspaceDto.ArtifactId, jobHistoryArtifactId);
		}
	}
}