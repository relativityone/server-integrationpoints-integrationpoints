using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
	public class TaggerTests : TestBase
	{
		private Tagger _instance;

		private IDocumentRepository _documentRepository;
		private IDataSynchronizer _dataSynchronizer;
		private string _importConfig;
		private int _sourceWorkspaceArtifactId;

		public override void SetUp()
		{
			_documentRepository = Substitute.For<IDocumentRepository>();
			_dataSynchronizer = Substitute.For<IDataSynchronizer>();
			IHelper helper = Substitute.For<IHelper>();

			FieldMap[] fields =
			{
				new FieldMap
				{
					DestinationField = new FieldEntry(),
					SourceField = new FieldEntry()
				},
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						DisplayName = "destination id",
						FieldIdentifier = "123456"
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					SourceField = new FieldEntry
					{
						DisplayName = "source id",
						FieldIdentifier = "789456"
					}
				}
			};

			_importConfig = string.Empty;
			_sourceWorkspaceArtifactId = 679185;

			_instance = new Tagger(_documentRepository, _dataSynchronizer, helper, fields, _importConfig, _sourceWorkspaceArtifactId);
		}

		[Test]
		public void ItShouldImportTaggingFieldsWhenThereAreDocumentsToTag()
		{
			//arrange
			SourceJobDTO job = new SourceJobDTO
			{
				Name = "whatever"
			};
			SourceWorkspaceDTO workspace = new SourceWorkspaceDTO
			{
				Name = "whatever"
			};

			var scratchTableRepository = Substitute.For<IScratchTableRepository>();
			scratchTableRepository.Count.Returns(1);

			TagsContainer tagsContainer = new TagsContainer(job, workspace);

			//act
			_instance.TagDocuments(tagsContainer, scratchTableRepository);

			//assert
			_dataSynchronizer.Received(1).SyncData(Arg.Any<IDataTransferContext>(), Arg.Any<FieldMap[]>(), _importConfig);
		}

		[Test]
		public void ItShouldNotImportTaggingFieldsWhenThereIsNoDocumentToTag()
		{
			//arrange
			SourceJobDTO job = new SourceJobDTO
			{
				Name = "whatever"
			};
			SourceWorkspaceDTO workspace = new SourceWorkspaceDTO
			{
				Name = "whatever"
			};

			var scratchTableRepository = Substitute.For<IScratchTableRepository>();
			scratchTableRepository.Count.Returns(0);

			TagsContainer tagsContainer = new TagsContainer(job, workspace);

			//act
			_instance.TagDocuments(tagsContainer, scratchTableRepository);

			//assert
			_dataSynchronizer.DidNotReceiveWithAnyArgs().SyncData(Arg.Any<IDataTransferContext>(), Arg.Any<FieldMap[]>(), _importConfig);
		}

		[Test]
		public void ItShouldAssignWorkspaceIdToDocumentRepository()
		{
			Assert.That(_documentRepository.WorkspaceArtifactId, Is.EqualTo(_sourceWorkspaceArtifactId));
		}
	}
}