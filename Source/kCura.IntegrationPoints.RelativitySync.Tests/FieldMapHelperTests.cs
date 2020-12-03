using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
	[TestFixture, Category("Unit")]
	internal sealed class FieldMapHelperTests
	{
		private ISerializer _serializer;

		[SetUp]
		public void SetUp()
		{
			_serializer = new JSONSerializer();
		}

		[Test]
		public void FixMappings_ShouldHandleEmptyMapping()
		{
			// ACT
			string result = FieldMapHelper.FixMappings(string.Empty, _serializer);

			// ASSERT
			Assert.That(result, Is.EqualTo(string.Empty));
		}

		[Test]
		public void FixMappings_ShouldHandleMappingWithoutIdentifier()
		{
			// ARRANGE
			List<FieldMap> fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						DisplayName = "abc"
					},
					SourceField = new FieldEntry
					{
						DisplayName = "abc [Object Identifier]"
					},
					FieldMapType = FieldMapTypeEnum.None
				}
			};

			string fieldMapping = _serializer.Serialize(fieldMap);

			// ACT
			string result = FieldMapHelper.FixMappings(fieldMapping, _serializer);

			// ASSERT
			Assert.That(result, Is.EqualTo(fieldMapping));
		}

		[Test]
		public void FixMappings_ShouldRemoveSuffixFromIdentifierMapping()
		{
			// ARRANGE
			List<FieldMap> fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						DisplayName = "abc [Object Identifier]"
					},
					SourceField = new FieldEntry
					{
						DisplayName = "abc [Object Identifier]"
					},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};

			string fieldMapping = _serializer.Serialize(fieldMap);

			// ACT
			string result = FieldMapHelper.FixMappings(fieldMapping, _serializer);

			List<FieldMap> modifiedMap = _serializer.Deserialize<List<FieldMap>>(result);

			// ASSERT
			Assert.That(modifiedMap[0].SourceField.DisplayName, Is.EqualTo("abc"));
			Assert.That(modifiedMap[0].DestinationField.DisplayName, Is.EqualTo("abc"));
		}

		[Test]
		public void FixMappings_ShouldRemoveSpecialFields()
		{
			// ARRANGE
			const FieldMapTypeEnum firstFieldMapTypeEnumToRemove = FieldMapTypeEnum.FolderPathInformation;
			const FieldMapTypeEnum secondFieldMapTypeEnumToRemove = FieldMapTypeEnum.NativeFilePath;
			const FieldMapTypeEnum fieldMapTypeNotToBeRemoved = FieldMapTypeEnum.Identifier;
			List<FieldMap> fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						DisplayName = "field"
					},
					SourceField = new FieldEntry
					{
						DisplayName = "Field"
					},
					FieldMapType = firstFieldMapTypeEnumToRemove
				},
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						DisplayName = "field"
					},
					SourceField = new FieldEntry
					{
						DisplayName = "Field"
					},
					FieldMapType = secondFieldMapTypeEnumToRemove
				},
				new FieldMap
				{
					DestinationField = new FieldEntry
					{
						DisplayName = "field"
					},
					SourceField = new FieldEntry
					{
						DisplayName = "Field"
					},
					FieldMapType = fieldMapTypeNotToBeRemoved
				}
			};

			string fieldMapping = _serializer.Serialize(fieldMap);

			// ACT
			string result = FieldMapHelper.FixMappings(fieldMapping, _serializer);

			List<FieldMap> modifiedMap = _serializer.Deserialize<List<FieldMap>>(result);

			// ASSERT
			Assert.That(modifiedMap.All(m => m.FieldMapType != firstFieldMapTypeEnumToRemove));
			Assert.That(modifiedMap.All(m => m.FieldMapType != secondFieldMapTypeEnumToRemove));

			Assert.That(modifiedMap.Any(m => m.FieldMapType == fieldMapTypeNotToBeRemoved));
		}

		[Test]
		public void FixMappings_ShouldDeduplicateFields()
		{
			// ARRANGE

			const string uniqueFieldIdentifier = "111";
			const string duplicatedFieldIdentifier = "222";

			List<FieldMap> fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "unique field",
						FieldIdentifier = uniqueFieldIdentifier
					},
					DestinationField = new FieldEntry
					{
						DisplayName = "unique field",
						FieldIdentifier = uniqueFieldIdentifier
					}
				},
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						DisplayName = "duplicated field",
						FieldIdentifier = duplicatedFieldIdentifier
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "duplicated field",
						FieldIdentifier = duplicatedFieldIdentifier
					}
				},
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						DisplayName = "duplicated field",
						FieldIdentifier = duplicatedFieldIdentifier
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "duplicated field",
						FieldIdentifier = duplicatedFieldIdentifier
					}
				}
			};

			string fieldMapping = _serializer.Serialize(fieldMap);

			// ACT
			string result = FieldMapHelper.FixMappings(fieldMapping, _serializer);

			List<FieldMap> modifiedMap = _serializer.Deserialize<List<FieldMap>>(result);

			// ASSERT
			modifiedMap.Count.Should().Be(2);
			modifiedMap.Single(x => x.SourceField.FieldIdentifier == uniqueFieldIdentifier &&
			                        x.DestinationField.FieldIdentifier == uniqueFieldIdentifier);
			modifiedMap.Single(x => x.SourceField.FieldIdentifier == duplicatedFieldIdentifier &&
			                        x.DestinationField.FieldIdentifier == duplicatedFieldIdentifier);
		}
	}
}