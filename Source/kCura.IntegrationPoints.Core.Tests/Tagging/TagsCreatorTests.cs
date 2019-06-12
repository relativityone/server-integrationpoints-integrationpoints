using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
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

		private IRelativitySourceJobRdoInitializer _sourceJobRdoInitializer;
		private IRelativitySourceWorkspaceRdoInitializer _sourceWorkspaceRdoInitializer;

		public override void SetUp()
		{
			_sourceJobManager = Substitute.For<ISourceJobManager>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
			_sourceJobRdoInitializer = Substitute.For<IRelativitySourceJobRdoInitializer>();
			_sourceWorkspaceRdoInitializer = Substitute.For<IRelativitySourceWorkspaceRdoInitializer>();

			var helper = Substitute.For<IHelper>();

			_instance = new TagsCreator(_sourceJobManager, _sourceWorkspaceManager, _sourceJobRdoInitializer, _sourceWorkspaceRdoInitializer, helper);
		}

		public void ItShouldCreateTags()
		{
			int sourceWorkspaceArtifactId = 843740;
			int destinationWorkspaceArtifactId = 527695;
			int jobHistoryArtifactId = 847715;
			int? federatedInstanceArtifactId = 561710;

			int sourceWorkspaceDescriptorArtifactTypeId = 625549;
			int sourceJobDescriptorArtifactTypeId = 801242;

			var sourceWorkspaceDto = new SourceWorkspaceDTO { ArtifactId = 831219 };
			var sourceJobDto = new SourceJobDTO();

			_sourceWorkspaceRdoInitializer.InitializeWorkspaceWithSourceWorkspaceRdo(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId)
				.Returns(sourceWorkspaceDescriptorArtifactTypeId);
			_sourceWorkspaceManager.CreateSourceWorkspaceDto(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, federatedInstanceArtifactId)
				.Returns(sourceWorkspaceDto);

			_sourceJobRdoInitializer.InitializeWorkspaceWithSourceJobRdo(destinationWorkspaceArtifactId, sourceWorkspaceDto.ArtifactTypeId).Returns(sourceJobDescriptorArtifactTypeId);
			_sourceJobManager.CreateSourceJobDto(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, jobHistoryArtifactId, sourceWorkspaceDto.ArtifactId)
				.Returns(sourceJobDto);

			// ACT
			var tagsContainer = _instance.CreateTags(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, jobHistoryArtifactId, federatedInstanceArtifactId);

			// ASSERT
			Assert.That(tagsContainer.SourceJobDto, Is.EqualTo(sourceJobDto));
			Assert.That(tagsContainer.SourceWorkspaceDto, Is.EqualTo(sourceWorkspaceDto));

			_sourceWorkspaceRdoInitializer.Received(1).InitializeWorkspaceWithSourceWorkspaceRdo(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);
			_sourceWorkspaceManager.Received(1)
				.CreateSourceWorkspaceDto(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, federatedInstanceArtifactId);

			_sourceJobRdoInitializer.Received(1).InitializeWorkspaceWithSourceJobRdo(destinationWorkspaceArtifactId, sourceWorkspaceDto.ArtifactTypeId);
			_sourceJobManager.Received(1)
				.CreateSourceJobDto(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, jobHistoryArtifactId, sourceWorkspaceDto.ArtifactId);
		}
	}
}