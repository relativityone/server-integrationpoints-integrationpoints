using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	internal sealed class SourceTagsFieldRowValueBuilderTests
	{
		[TestCase(SpecialFieldType.SourceJob)]
		[TestCase(SpecialFieldType.SourceWorkspace)]
		public void ItShouldAllowCorrectSpecialFieldType(SpecialFieldType fieldType)
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>();
			var instance = new SourceTagsFieldRowValuesBuilder(configuration.Object);

			// Act
			IEnumerable<SpecialFieldType> allowedSpecialFieldTypes = instance.AllowedSpecialFieldTypes;

			// Assert
			allowedSpecialFieldTypes.Should().Contain(fieldType);
		}

		[TestCase(SpecialFieldType.SourceJob, "SourceJobTag")]
		[TestCase(SpecialFieldType.SourceWorkspace, "SourceWorkspaceTag")]
		public void ItShouldReturnCorrectValueFromConfigurationBasedOnType(SpecialFieldType fieldType, object expectedResult)
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>();
			configuration.SetupGet(x => x.SourceJobTagName).Returns("SourceJobTag");
			configuration.SetupGet(x => x.SourceWorkspaceTagName).Returns("SourceWorkspaceTag");

			var instance = new SourceTagsFieldRowValuesBuilder(configuration.Object);

			FieldInfoDto fieldInfo = FieldInfoDto.GenericSpecialField(fieldType, "foo");
			var document = new RelativityObjectSlim();

			// Act
			object result = instance.BuildRowValue(fieldInfo, document, null);

			// Assert
			result.Should().Be(expectedResult);
		}

		[Test]
		public void ItShouldThrowArgumentExceptionWhenInvalidFieldTypeIsGiven()
		{
			// Arrange
			var configuration = new Mock<ISynchronizationConfiguration>();
			configuration.SetupGet(x => x.SourceJobTagName).Returns("SourceJobTag");
			configuration.SetupGet(x => x.SourceWorkspaceTagName).Returns("SourceWorkspaceTag");

			var instance = new SourceTagsFieldRowValuesBuilder(configuration.Object);

			FieldInfoDto fieldInfo = FieldInfoDto.NativeFileLocationField();
			var document = new RelativityObjectSlim();

			// Act
			Action action = () => instance.BuildRowValue(fieldInfo, document, null);

			// Assert
			action.Should().Throw<ArgumentException>()
				.Which.Message.Should().MatchRegex(@".*SpecialFieldType\.NativeFileLocation.*");
		}
	}
}
