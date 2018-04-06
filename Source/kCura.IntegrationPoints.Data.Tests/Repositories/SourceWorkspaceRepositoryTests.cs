using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	public class SourceWorkspaceRepositoryTests : TestBase
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IFieldRepository _fieldRepository;
		private IRdoRepository _rdoRepository;
		private IHelper _helper;

		private IAPILog _logApi;

		private SourceWorkspaceRepository _instance;

		public override void SetUp()
		{
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_fieldRepository = Substitute.For<IFieldRepository>();
			_rdoRepository = Substitute.For<IRdoRepository>();
			_helper = Substitute.For<IHelper>();
			_logApi = Substitute.For<IAPILog>();

			_instance = new SourceWorkspaceRepository(_helper, _objectTypeRepository, _fieldRepository, _rdoRepository);
		}

		[Test]
		public void ItShouldCreateObjectType()
		{
			var expectedResult = 554395;

			var parentArtifactTypeId = 275;

			_objectTypeRepository.CreateObjectType(SourceWorkspaceDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, parentArtifactTypeId).Returns(expectedResult);

			// ACT
			var actualResult = _instance.CreateObjectType(parentArtifactTypeId);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));
			_objectTypeRepository.Received(1).CreateObjectType(SourceWorkspaceDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, parentArtifactTypeId);
		}

		[Test]
		public void ItShouldCreate()
		{
			var expectedResult = 268763;

			var sourceWorkspaceDto = new SourceWorkspaceDTO
			{
				ArtifactTypeId = 339125
			};

			_rdoRepository.Create(Arg.Any<RDO>()).Returns(expectedResult);

			// ACT
			var actualResult = _instance.Create(sourceWorkspaceDto);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));
			_rdoRepository.Received(1).Create(Arg.Is<RDO>(x => x.ArtifactID == 0 && x.ArtifactTypeID == sourceWorkspaceDto.ArtifactTypeId));
		}

		[Test]
		public void ItShouldUpdate()
		{
			var sourceWorkspaceDto = new SourceWorkspaceDTO
			{
				ArtifactId = 199398,
				ArtifactTypeId = 260895
			};

			// ACT
			_instance.Update(sourceWorkspaceDto);

			// ASSERT
			_rdoRepository.Received(1).Update(Arg.Is<RDO>(x => x.ArtifactID == sourceWorkspaceDto.ArtifactId && x.ArtifactTypeID == sourceWorkspaceDto.ArtifactTypeId));
		}

		[Test]
		public void ItShouldCreateFieldOnDocument()
		{
			var expectedResult = 990339;

			int sourceWorkspaceObjectTypeId = 214619;

			_fieldRepository.CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, sourceWorkspaceObjectTypeId).Returns(expectedResult);

			// ACT
			var actualResult = _instance.CreateFieldOnDocument(sourceWorkspaceObjectTypeId);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));
			_fieldRepository.Received(1).CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, sourceWorkspaceObjectTypeId);
		}

		[Test]
		public void ItShouldRetrieveForSourceWorkspaceId()
		{
			var rdo = new RDO(348296)
			{
				Fields = new List<FieldValue>()
			};

			var expectedResult = new SourceWorkspaceDTO
			{
				ArtifactId = rdo.ArtifactID
			};

			_rdoRepository.QuerySingle(Arg.Any<Query<RDO>>()).Returns(rdo);

			// ACT
			var actualResult = _instance.RetrieveForSourceWorkspaceId(156272, "fed_name_503", 541);

			// ASSERT
			Assert.That(actualResult.ArtifactId, Is.EqualTo(expectedResult.ArtifactId));
			_rdoRepository.Received(1)
				.QuerySingle(
					Arg.Is<Query<RDO>>(x =>
						x.ArtifactTypeGuid == SourceWorkspaceDTO.ObjectTypeGuid
						&& x.Fields[0].Name == "*"
					));
		}

		[Test]
		public void ItShouldThrowExceptionWhenRetrieveForSourceWorkspaceId()
		{
			_rdoRepository.QuerySingle(Arg.Any<Query<RDO>>()).Throws(new Exception());

			// ACT
			Assert.Throws<Exception>(() => _instance.RetrieveForSourceWorkspaceId(156272, "fed_name_503", 541));

			// ASSERT

			_logApi.LogError(Arg.Any<string>(), Arg.Any<string>());
		}
	}
}