using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Field = kCura.Relativity.Client.DTOs.Field;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySourceRdo
{
	public class RelativitySourceWorkspaceRdoInitializerTests : TestBase
	{
		private const int _DESTINATION_WORKSPACE_ID = 581555;
		private const int _SOURCE_WORKSPACE_ID = 649601;

		private ISourceWorkspaceRepository _sourceWorkspaceRepository;
		private IRelativitySourceRdoObjectType _relativitySourceRdoObjectType;
		private IRelativitySourceRdoDocumentField _relativitySourceRdoDocumentField;
		private IRelativitySourceRdoFields _relativitySourceRdoFields;

		private RelativitySourceWorkspaceRdoInitializer _instance;

		public override void SetUp()
		{
			_sourceWorkspaceRepository = Substitute.For<ISourceWorkspaceRepository>();
			_relativitySourceRdoObjectType = Substitute.For<IRelativitySourceRdoObjectType>();
			_relativitySourceRdoDocumentField = Substitute.For<IRelativitySourceRdoDocumentField>();
			_relativitySourceRdoFields = Substitute.For<IRelativitySourceRdoFields>();

			IHelper helper = Substitute.For<IHelper>();
			IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
			IRelativitySourceRdoHelpersFactory helpersFactory = Substitute.For<IRelativitySourceRdoHelpersFactory>();

			repositoryFactory.GetSourceWorkspaceRepository(_DESTINATION_WORKSPACE_ID).Returns(_sourceWorkspaceRepository);

			helpersFactory.CreateRelativitySourceRdoDocumentField(_sourceWorkspaceRepository).Returns(_relativitySourceRdoDocumentField);
			helpersFactory.CreateRelativitySourceRdoFields().Returns(_relativitySourceRdoFields);
			helpersFactory.CreateRelativitySourceRdoObjectType(_sourceWorkspaceRepository).Returns(_relativitySourceRdoObjectType);

			_instance = new RelativitySourceWorkspaceRdoInitializer(helper, repositoryFactory, helpersFactory);
		}

		[Test]
		public void ItShouldInitializeDestinationWorkspace()
		{
			var expectedSourceJobDescriptorId = 612268;

			_relativitySourceRdoObjectType.CreateObjectType(_DESTINATION_WORKSPACE_ID, SourceWorkspaceDTO.ObjectTypeGuid,
					IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, (int) ArtifactType.Case)
				.Returns(expectedSourceJobDescriptorId);

			// ACT
			var actualSourceJobDescriptorId = _instance.InitializeWorkspaceWithSourceWorkspaceRdo(_SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID);

			// ASSERT
			Assert.That(actualSourceJobDescriptorId, Is.EqualTo(expectedSourceJobDescriptorId));

			_relativitySourceRdoObjectType.Received(1)
				.CreateObjectType(_DESTINATION_WORKSPACE_ID, SourceWorkspaceDTO.ObjectTypeGuid, IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, (int) ArtifactType.Case);
			_relativitySourceRdoFields.Received(1).CreateFields(_DESTINATION_WORKSPACE_ID, Arg.Any<IDictionary<Guid, Field>>());
			_relativitySourceRdoDocumentField.Received(1).CreateDocumentField(_DESTINATION_WORKSPACE_ID, SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid,
				IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, expectedSourceJobDescriptorId);
		}
	}
}