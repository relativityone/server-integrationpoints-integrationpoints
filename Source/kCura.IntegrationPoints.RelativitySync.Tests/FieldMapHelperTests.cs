﻿using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
	[TestFixture]
	internal sealed class FieldMapHelperTests
	{
		private ISerializer _serializer;

		[SetUp]
		public void SetUp()
		{
			_serializer = new JSONSerializer();
		}

		[Test]
		public void ItShouldHandleEmptyMapping()
		{
			// ACT
			string result = FieldMapHelper.FixControlNumberFieldName(string.Empty, _serializer);

			// ASSERT
			Assert.That(result, Is.EqualTo(string.Empty));
		}

		[Test]
		public void ItShouldHandleMappingWithoutIdentifier()
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
			string result = FieldMapHelper.FixControlNumberFieldName(fieldMapping, _serializer);

			// ASSERT
			Assert.That(result, Is.EqualTo(fieldMapping));
		}

		[Test]
		public void ItShouldRemoveSuffixFromIdentifierMapping()
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
			string result = FieldMapHelper.FixControlNumberFieldName(fieldMapping, _serializer);

			List<FieldMap> modifiedMap = _serializer.Deserialize<List<FieldMap>>(result);

			// ASSERT
			Assert.That(modifiedMap[0].SourceField.DisplayName, Is.EqualTo("abc"));
			Assert.That(modifiedMap[0].DestinationField.DisplayName, Is.EqualTo("abc"));
		}
	}
}