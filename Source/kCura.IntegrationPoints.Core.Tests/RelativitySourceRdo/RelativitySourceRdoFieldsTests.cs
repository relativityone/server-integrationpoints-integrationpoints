using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Field = kCura.Relativity.Client.DTOs.Field;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySourceRdo
{
	public class RelativitySourceRdoFieldsTests : TestBase
	{
		private const int _WORKSPACE_ID = 216578;

		private IFieldQueryRepository _fieldQueryRepository;
		private IArtifactGuidRepository _artifactGuidRepository;
		private IFieldRepository _fieldRepository;

		private RelativitySourceRdoFields _instance;

		public override void SetUp()
		{
			_fieldQueryRepository = Substitute.For<IFieldQueryRepository>();
			_artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
			_fieldRepository = Substitute.For<IFieldRepository>();

			IRepositoryFactory repositoryFactory = Substitute.For<IRepositoryFactory>();
			repositoryFactory.GetArtifactGuidRepository(_WORKSPACE_ID).Returns(_artifactGuidRepository);
			repositoryFactory.GetFieldQueryRepository(_WORKSPACE_ID).Returns(_fieldQueryRepository);
			repositoryFactory.GetFieldRepository(_WORKSPACE_ID).Returns(_fieldRepository);

			_instance = new RelativitySourceRdoFields(repositoryFactory);
		}

		[Test]
		public void ItShouldCreateNonExistingFields()
		{
			var fieldGuid = Guid.NewGuid();
			var fieldName = "field_name_316";
			var descriptorArtifactTypeID = 100138;

			var fieldId = 444186;

			IDictionary<Guid, Field> fields = new Dictionary<Guid, Field>
			{
				{
					fieldGuid, new Field
					{
						Name = fieldName,
						FieldTypeID = FieldType.WholeNumber,
						ObjectType = new ObjectType
						{
							DescriptorArtifactTypeID = descriptorArtifactTypeID
						}
					}
				}
			};

			_artifactGuidRepository.GuidExists(fieldGuid).Returns(false);
			_fieldQueryRepository.RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(),
					Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)))
				.Returns((ArtifactDTO) null);
			_fieldRepository.CreateObjectTypeField(fields[fieldGuid]).Returns(fieldId);

			// ACT
			_instance.CreateFields(_WORKSPACE_ID, fields);

			// ASSERT
			_artifactGuidRepository.Received(1).GuidExists(fieldGuid);
			_fieldQueryRepository.Received(1)
				.RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(), Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)));
			_fieldRepository.Received(1).CreateObjectTypeField(fields[fieldGuid]);
			_artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(fieldId, fieldGuid);
		}

		[Test]
		public void ItShouldUpdateExistingFields()
		{
			var fieldGuid = Guid.NewGuid();
			var fieldName = "field_name_526";
			var descriptorArtifactTypeID = 488469;

			var fieldId = 431240;

			IDictionary<Guid, Field> fields = new Dictionary<Guid, Field>
			{
				{
					fieldGuid, new Field
					{
						Name = fieldName,
						FieldTypeID = FieldType.WholeNumber,
						ObjectType = new ObjectType
						{
							DescriptorArtifactTypeID = descriptorArtifactTypeID
						}
					}
				}
			};

			_artifactGuidRepository.GuidExists(fieldGuid).Returns(false);
			_fieldQueryRepository.RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(),
					Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)))
				.Returns(new ArtifactDTO(fieldId, 744, "", new List<ArtifactFieldDTO>()));

			// ACT
			_instance.CreateFields(_WORKSPACE_ID, fields);

			// ASSERT
			_artifactGuidRepository.Received(1).GuidExists(fieldGuid);
			_fieldQueryRepository.Received(2)
				.RetrieveField(descriptorArtifactTypeID, fieldName, fields[fieldGuid].GetFieldTypeName(), Arg.Is<HashSet<string>>(x => x.Contains(Constants.Fields.ArtifactId)));
			_artifactGuidRepository.Received(1).InsertArtifactGuidForArtifactId(fieldId, fieldGuid);

			_fieldRepository.DidNotReceive().CreateObjectTypeField(fields[fieldGuid]);
		}

		[Test]
		public void ItShouldSkipCreationForExistingField()
		{
			var fieldGuid = Guid.NewGuid();

			IDictionary<Guid, Field> fields = new Dictionary<Guid, Field>
			{
				{
					fieldGuid, new Field
					{
						Name = "field_name_246",
						FieldTypeID = FieldType.WholeNumber,
						ObjectType = new ObjectType
						{
							DescriptorArtifactTypeID = 402331
						}
					}
				}
			};

			_artifactGuidRepository.GuidExists(fieldGuid).Returns(true);

			// ACT
			_instance.CreateFields(_WORKSPACE_ID, fields);

			// ASSERT
			_artifactGuidRepository.Received(1).GuidExists(fieldGuid);

			_fieldQueryRepository.DidNotReceive().RetrieveField(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<HashSet<string>>());
			_fieldRepository.DidNotReceive().CreateObjectTypeField(Arg.Any<Field>());
			_artifactGuidRepository.DidNotReceive().InsertArtifactGuidForArtifactId(Arg.Any<int>(), Arg.Any<Guid>());
		}
	}
}