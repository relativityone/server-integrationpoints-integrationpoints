using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
	[TestFixture]
	public class FieldsMappingValidatorTests
	{
		private IFieldsMappingValidator _sut;

		private Mock<IFieldsClassifyRunnerFactory> _fieldsClassifyRunnerFactoryFake;
		private Mock<IFieldsClassifierRunner> _fieldClassifierRunnerMock;

		private const int _SOURCE_WORKSPACE_ID = 1;
		private const int _DESTINATION_WORKSPACE_ID = 2;
		private const int _SOURCE_ARTIFACT_TYPE_ID = 3;
		private const int _DESTINATION_ARTIFACT_TYPE_ID = 4;

		[SetUp]
		public void SetUp()
		{
			_fieldClassifierRunnerMock = new Mock<IFieldsClassifierRunner>();

			_fieldsClassifyRunnerFactoryFake = new Mock<IFieldsClassifyRunnerFactory>();
			_fieldsClassifyRunnerFactoryFake.Setup(m => m.CreateForSourceWorkspace(_SOURCE_ARTIFACT_TYPE_ID))
				.Returns(_fieldClassifierRunnerMock.Object);
			_fieldsClassifyRunnerFactoryFake.Setup(m => m.CreateForDestinationWorkspace(_DESTINATION_ARTIFACT_TYPE_ID))
				.Returns(_fieldClassifierRunnerMock.Object);

			_sut = new FieldsMappingValidator(_fieldsClassifyRunnerFactoryFake.Object);
		}

		[TestCase(255, 255, true)]
		[TestCase(255, 50, false)]
		public async Task ValidateAsync_ShouldValidateObjectIdentifier(int sourceLength, int destinationLength, bool expectValid)
		{
			var IDs = new List<string> { "1", "2" };
			FieldInfo sourceControlNumber = CreateWithType(IDs[0], $"{FieldTypeName.FIXED_LENGTH_TEXT}({sourceLength})");
			FieldInfo destinationControlNumber = CreateWithType(IDs[1], $"{FieldTypeName.FIXED_LENGTH_TEXT}({destinationLength})");

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(sourceControlNumber) { ClassificationLevel = ClassificationLevel.AutoMap }
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(destinationControlNumber) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(sourceControlNumber),
					DestinationField = FieldConvert.ToFieldEntry(destinationControlNumber),
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			// Act
			FieldMappingValidationResult result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.IsObjectIdentifierMapValid.Should().Be(expectValid);
		}

		[Test]
		public async Task ValidateAsync_ShouldBeEmpty_WhenFieldMapIsNull()
		{
			// Act
			var result = await _sut.ValidateAsync(null, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();

			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForSourceWorkspace(It.IsAny<int>()), Times.Never);
			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForDestinationWorkspace(It.IsAny<int>()), Times.Never);
		}

		[Test]
		public async Task ValidateAsync_ShouldBeEmpty_WhenFieldMapIsEmpty()
		{
			// Act
			var result = await _sut.ValidateAsync(Enumerable.Empty<FieldMap>(), _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();

			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForSourceWorkspace(It.IsAny<int>()), Times.Never);
			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForDestinationWorkspace(It.IsAny<int>()), Times.Never);
		}

		[Test]
		public async Task ValidateAsync_ShouldAlwaysCallFieldClassificationForMappedFieldsOnly()
		{
			List<string> IDs = new List<string> { "1", "2", "3", "4", "5" };
			var field1 = CreateWithType(IDs[0], FieldTypeName.DATE);
			var field2 = CreateWithType(IDs[1], FieldTypeName.DECIMAL);
			var field3 = CreateWithType(IDs[2], $"{FieldTypeName.FIXED_LENGTH_TEXT}(255)");
			var field4 = CreateWithType(IDs[3], $"{FieldTypeName.FIXED_LENGTH_TEXT}(20)");
			var field5 = CreateWithType(IDs[4], FieldTypeName.LONG_TEXT);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field2) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field3) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field4) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field5) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field2) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field3) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field4) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field5) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field3),
					DestinationField = FieldConvert.ToFieldEntry(field4)
				},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			// Act
			await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			_fieldClassifierRunnerMock.Verify(m => m.ClassifyFieldsAsync(new List<string>() { "1", "3" }, _SOURCE_WORKSPACE_ID), Times.Once);
			_fieldClassifierRunnerMock.Verify(m => m.ClassifyFieldsAsync(new List<string>() { "2", "4" }, _DESTINATION_WORKSPACE_ID), Times.Once);

		}

		[Test]
		public async Task ValidateAsync_ShouldNotReturnInvalidVields_WhenTypesAreCompatible()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2", "3", "4", "5" };

			var field1 = CreateWithType(IDs[0], FieldTypeName.DATE);
			var field2 = CreateWithType(IDs[1], FieldTypeName.DECIMAL);
			var field3 = CreateWithType(IDs[2], $"{FieldTypeName.FIXED_LENGTH_TEXT}(255)");
			var field4 = CreateWithType(IDs[3], $"{FieldTypeName.FIXED_LENGTH_TEXT}(20)");
			var field5 = CreateWithType(IDs[4], FieldTypeName.LONG_TEXT);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field2) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field3) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field4) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field5) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field2) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field3) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field4) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field5) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field4),
					DestinationField = FieldConvert.ToFieldEntry(field3)
				},
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();

		}

		[Test]
		public async Task ValidateAsync_ShouldReturnInvalidFields_WhenTypesAreNotCompatible()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2", "3", "4", "5" };

			var field1 = CreateWithType(IDs[0], FieldTypeName.DATE);
			var field2 = CreateWithType(IDs[1], FieldTypeName.DECIMAL);
			var field3 = CreateWithType(IDs[2], $"{FieldTypeName.FIXED_LENGTH_TEXT}(255)");
			var field4 = CreateWithType(IDs[3], $"{FieldTypeName.FIXED_LENGTH_TEXT}(20)");
			var field5 = CreateWithType(IDs[4], FieldTypeName.LONG_TEXT);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field2) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field3) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field4) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field5) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field2) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field3) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field4) { ClassificationLevel = ClassificationLevel.AutoMap},
				new FieldClassificationResult(field5) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field3),
					DestinationField = FieldConvert.ToFieldEntry(field4)
				},
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count);
			result.InvalidMappedFields.Select(x => x.FieldMap)
				.Should().Contain(fieldMap);
		}

		public static IEnumerable<TestCaseData> UnicodeDependentTestCaseSource()
		{
			yield return new TestCaseData(
				new FieldInfo("1", "Field1", FieldTypeName.MULTIPLE_CHOICE) { Unicode = true },
				new FieldInfo("1", "Field1", FieldTypeName.MULTIPLE_CHOICE) { Unicode = false }
			);
			yield return new TestCaseData(
				new FieldInfo("1", "Field1", FieldTypeName.SINGLE_CHOICE) { Unicode = true },
				new FieldInfo("1", "Field1", FieldTypeName.SINGLE_CHOICE) { Unicode = false }
			);
			yield return new TestCaseData(
				new FieldInfo("1", "Field1", FieldTypeName.LONG_TEXT) { Unicode = true },
				new FieldInfo("1", "Field1", FieldTypeName.LONG_TEXT) { Unicode = false }
			);
			yield return new TestCaseData(
				new FieldInfo("1", "Field1", FieldTypeName.FIXED_LENGTH_TEXT) { Unicode = true },
				new FieldInfo("1", "Field1", FieldTypeName.FIXED_LENGTH_TEXT) { Unicode = false }
			);
		}

		[TestCaseSource(nameof(UnicodeDependentTestCaseSource))]
		public async Task ValidateAsync_ShouldReturnInvalidFields_WhenUnicodeIsDifferentAndTypeIsUnicodeDependent(FieldInfo sourceField, FieldInfo destinationField)
		{
			// Arrange
			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(sourceField) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(destinationField) { ClassificationLevel = ClassificationLevel.AutoMap},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(sourceField),
					DestinationField = FieldConvert.ToFieldEntry(destinationField)
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count);
			result.InvalidMappedFields.Select(x => x.FieldMap)
				.Should().Contain(fieldMap);
		}

		[Test]
		public async Task ValidateAsync_ShouldNotReturnInvalidFields_WhenMappedFieldsMeetClassificationRestriction()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2"};

			var field1 = CreateWithSampleType(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldReturnWholeFieldMapAsInvalid_WhenMappedFieldsDontMeetSourceClassificationRestriction()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithSampleType(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.HideFromUser
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.ShowToUser
				},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count);
			result.InvalidMappedFields.Select(x => x.FieldMap)
				.Should().Contain(fieldMap);
		}

		[Test]
		public async Task ValidateAsync_ShouldReturnFieldMapAsInvalid_IfMappedFieldsAreNotInWorkspaces()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithSampleType(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				}
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count);
			result.InvalidMappedFields.Select(x => x.FieldMap)
				.Should().Contain(fieldMap);
		}

		[Test]
		public async Task ValidateAsync_ShouldReturnWholeFieldMapAsInvalid_WhenMappedFieldsDontMeetDestinationClassificationRestriction()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithSampleType(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.ShowToUser
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.HideFromUser
				},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count);
			result.InvalidMappedFields.Select(x => x.FieldMap)
				.Should().Contain(fieldMap);
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidation_WhenFieldsMappingContainSpecialFieldAndIsMapped()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithSampleType(IDs[0]);
			var field2 = CreateWithType(IDs[1], "Long Text");

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = EmptyField(),
					FieldMapType = FieldMapTypeEnum.FolderPathInformation
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidation_WhenSomeFieldInSourceClassificationIsMissing()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithSampleType(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().NotBeEmpty()
				.And.Contain(f => f.FieldMap.DestinationField.FieldIdentifier == field2.FieldIdentifier);
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidation_WhenSomeFieldInDestinationClassificationIsMissing()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithSampleType(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				}
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1)
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2)
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().NotBeEmpty()
				.And.Contain(f => f.FieldMap.DestinationField.FieldIdentifier == field2.FieldIdentifier);
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidation_WhenOnlyObjectIdentifierIsMapped()
		{
			// Arrange
			List<string> IDs = new List<string> { "1" };

			var field1 = CreateWithIdentifier(IDs[0]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				}
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				}
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1),
					FieldMapType = FieldMapTypeEnum.Identifier
				},
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidation_WhenOnlyObjectIdentifierIsMappedAndFolderPathInformationIsSet()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithIdentifier(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.ShowToUser
				}
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				}
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1),
					FieldMapType = FieldMapTypeEnum.Identifier
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = EmptyField(),
					FieldMapType = FieldMapTypeEnum.FolderPathInformation
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidation_WhenFolderPathInformationIsSetAndSameFieldIsMapped()
		{
			// Arrange
			List<string> IDs = new List<string> { "1", "2" };

			var field1 = CreateWithIdentifier(IDs[0]);
			var field2 = CreateWithSampleType(IDs[1]);

			var sourceClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult(field2)
				{
					ClassificationLevel = ClassificationLevel.ShowToUser
				}
			};

			var destinationClassifiedFields = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(field1)
				{
					ClassificationLevel = ClassificationLevel.AutoMap
				}
			};

			LoadFieldClassifierRunnerWithData(sourceClassifiedFields, destinationClassifiedFields);

			var fieldMap = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field1),
					DestinationField = FieldConvert.ToFieldEntry(field1),
					FieldMapType = FieldMapTypeEnum.Identifier
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2),
					FieldMapType = FieldMapTypeEnum.FolderPathInformation
				}
			};

			// Act
			var result = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID, _SOURCE_ARTIFACT_TYPE_ID, _DESTINATION_ARTIFACT_TYPE_ID).ConfigureAwait(false);

			// Assert
			result.InvalidMappedFields.Should().BeEmpty();
		}

		private FieldInfo CreateWithSampleType(string id)
		{
			return new FieldInfo(id, $"Field {id}", "Sample Type");
		}

		private FieldInfo CreateWithType(string id, string type)
		{
			return new FieldInfo(id, $"Field {id}", type);
		}

		private FieldInfo CreateWithIdentifier(string id)
		{
			return new FieldInfo(id, $"Field {id}", "Sample Type")
			{
				IsIdentifier = true
			};
		}

		private FieldEntry EmptyField() => new FieldEntry();

		private void LoadFieldClassifierRunnerWithData(IEnumerable<FieldClassificationResult> sourceClassifiedFields, IEnumerable<FieldClassificationResult> destinationClassifiedFields)
		{
			_fieldClassifierRunnerMock.Setup(m => m.ClassifyFieldsAsync(It.IsAny<ICollection<string>>(), _SOURCE_WORKSPACE_ID))
				.ReturnsAsync(sourceClassifiedFields);
			_fieldClassifierRunnerMock.Setup(m => m.ClassifyFieldsAsync(It.IsAny<ICollection<string>>(), _DESTINATION_WORKSPACE_ID))
				.ReturnsAsync(destinationClassifiedFields);
		}
	}
}
