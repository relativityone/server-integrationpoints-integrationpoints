﻿using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
	public class TaggerTests : TestBase
	{
		private Tagger _instance;

		private IDocumentRepository _documentRepository;
		private IDataSynchronizer _dataSynchronizer;
		private string _importConfig;

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

			_instance = new Tagger(_documentRepository, _dataSynchronizer, helper, fields, _importConfig);
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
			scratchTableRepository.GetCount().Returns(1);

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
			scratchTableRepository.GetCount().Returns(0);

			TagsContainer tagsContainer = new TagsContainer(job, workspace);

			//act
			_instance.TagDocuments(tagsContainer, scratchTableRepository);

			//assert
			_dataSynchronizer.DidNotReceiveWithAnyArgs().SyncData(Arg.Any<IDataTransferContext>(), Arg.Any<FieldMap[]>(), _importConfig);
		}
	}
}