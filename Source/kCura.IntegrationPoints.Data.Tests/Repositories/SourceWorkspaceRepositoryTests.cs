using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	public class SourceWorkspaceRepositoryTests : TestBase
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IFieldRepository _fieldRepository;
		private IRdoRepository _rdoRepository;

		private SourceWorkspaceRepository _instance;

		public override void SetUp()
		{
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_fieldRepository = Substitute.For<IFieldRepository>();
			_rdoRepository = Substitute.For<IRdoRepository>();
			_instance = new SourceWorkspaceRepository(_objectTypeRepository, _fieldRepository, _rdoRepository);
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
		public void ItShouldCreateObjectTypeFields()
		{
			IDictionary<Guid, int> expectedResult = new Dictionary<Guid, int>
			{
				{SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, 186267},
				{SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid, 694575}
			};

			int sourceWorkspaceObjectTypeId = 213495;
			IEnumerable<Guid> fieldGuids = new[] {SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid};

			_fieldRepository.CreateObjectTypeFields(Arg.Any<List<Field>>()).Returns(expectedResult.Select(x => new Field(x.Value) {Guids = new List<Guid> {x.Key}}).ToList());

			// ACT
			var actualResult = _instance.CreateObjectTypeFields(sourceWorkspaceObjectTypeId, fieldGuids);

			// ASSERT
			Assert.That(actualResult.All(x => expectedResult.Keys.Contains(x.Key) && expectedResult[x.Key] == x.Value));

			_fieldRepository.Received(1)
				.CreateObjectTypeFields(Arg.Is<List<Field>>(x => x.All(y => y.ObjectType.DescriptorArtifactTypeID == sourceWorkspaceObjectTypeId) && x.Count == fieldGuids.Count()));
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
	}
}