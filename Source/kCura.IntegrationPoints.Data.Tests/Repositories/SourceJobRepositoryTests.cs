﻿using System;
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
	public class SourceJobRepositoryTests : TestBase
	{
		private IObjectTypeRepository _objectTypeRepository;
		private IFieldRepository _fieldRepository;
		private IRdoRepository _rdoRepository;

		private SourceJobRepository _instance;

		public override void SetUp()
		{
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_fieldRepository = Substitute.For<IFieldRepository>();
			_rdoRepository = Substitute.For<IRdoRepository>();
			_instance = new SourceJobRepository(_objectTypeRepository, _fieldRepository, _rdoRepository);
		}

		[Test]
		public void ItShouldCreateObjectType()
		{
			var expectedResult = 140653;

			var parentArtifactTypeId = 268;

			_objectTypeRepository.CreateObjectType(SourceJobDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, parentArtifactTypeId).Returns(expectedResult);

			// ACT
			var actualResult = _instance.CreateObjectType(parentArtifactTypeId);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));
			_objectTypeRepository.Received(1).CreateObjectType(SourceJobDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, parentArtifactTypeId);
		}

		[Test]
		public void ItShouldCreate()
		{
			var expectedResult = 855108;

			var sourceJobDto = new SourceJobDTO
			{
				ArtifactTypeId = 880951
			};

			_rdoRepository.Create(Arg.Any<RDO>()).Returns(expectedResult);

			// ACT
			var actualResult = _instance.Create(sourceJobDto);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));
			_rdoRepository.Received(1).Create(Arg.Is<RDO>(x => x.ArtifactID == 0 && x.ArtifactTypeID == sourceJobDto.ArtifactTypeId));
		}

		[Test]
		public void ItShouldCreateFieldOnDocument()
		{
			var expectedResult = 524116;

			int sourceJobArtifactTypeId = 362279;

			_fieldRepository.CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, sourceJobArtifactTypeId).Returns(expectedResult);

			// ACT
			var actualResult = _instance.CreateFieldOnDocument(sourceJobArtifactTypeId);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));
			_fieldRepository.Received(1).CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, sourceJobArtifactTypeId);
		}

		[Test]
		public void ItShouldCreateObjectTypeFields()
		{
			IDictionary<Guid, int> expectedResult = new Dictionary<Guid, int>
			{
				{SourceJobDTO.Fields.JobHistoryIdFieldGuid, 545306},
				{SourceJobDTO.Fields.JobHistoryNameFieldGuid, 264144}
			};

			int sourceJobArtifactTypeId = 392929;
			IEnumerable<Guid> fieldGuids = new[] { SourceJobDTO.Fields.JobHistoryIdFieldGuid, SourceJobDTO.Fields.JobHistoryNameFieldGuid };

			_fieldRepository.CreateObjectTypeFields(Arg.Any<List<Field>>()).Returns(expectedResult.Select(x => new Field(x.Value) {Guids = new List<Guid> {x.Key}}).ToList());

			// ACT
			var actualResult = _instance.CreateObjectTypeFields(sourceJobArtifactTypeId, fieldGuids);

			// ASSERT
			Assert.That(actualResult.All(x => expectedResult.Keys.Contains(x.Key) && expectedResult[x.Key] == x.Value));

			_fieldRepository.Received(1)
				.CreateObjectTypeFields(Arg.Is<List<Field>>(x => x.All(y => y.ObjectType.DescriptorArtifactTypeID == sourceJobArtifactTypeId) && x.Count == fieldGuids.Count()));
		}
	}
}