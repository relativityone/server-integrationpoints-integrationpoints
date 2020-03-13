//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		[SetUp]
		public void SetUp()
		{
			_fieldClassifierRunnerMock = new Mock<IFieldsClassifierRunner>();

			_fieldsClassifyRunnerFactoryFake = new Mock<IFieldsClassifyRunnerFactory>();
			_fieldsClassifyRunnerFactoryFake.Setup(m => m.CreateForSourceWorkspace())
				.Returns(_fieldClassifierRunnerMock.Object);
			_fieldsClassifyRunnerFactoryFake.Setup(m => m.CreateForDestinationWorkspace())
				.Returns(_fieldClassifierRunnerMock.Object);

			_sut = new FieldsMappingValidator(_fieldsClassifyRunnerFactoryFake.Object);
		}

		[Test]
		public async Task ValidateAsync_ShouldBeEmpty_WhenFieldMapIsNull()
		{
			// Act
			var invalidMappedFields = await _sut.ValidateAsync(null, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();

			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForSourceWorkspace(), Times.Never);
			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForDestinationWorkspace(), Times.Never);
		}

		[Test]
		public async Task ValidateAsync_ShouldBeEmpty_WhenFieldMapIsEmpty()
		{
			// Act
			var invalidMappedFields = await _sut.ValidateAsync(Enumerable.Empty<FieldMap>(), _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();

			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForSourceWorkspace(), Times.Never);
			_fieldsClassifyRunnerFactoryFake.Verify(m => m.CreateForDestinationWorkspace(), Times.Never);
		}

		[Test]
		public async Task ValidateAsync_ShouldAlwaysCallFieldClassificationForMappedFieldsOnly()
		{
			List<string> IDs = new List<string> { "1", "2", "3", "4","5"};
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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();

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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count)
				.And.Contain(fieldMap);
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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();
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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count)
				.And.Contain(fieldMap);
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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count)
				.And.Contain(fieldMap);
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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().NotBeEmpty()
				.And.HaveCount(fieldMap.Count)
				.And.Contain(fieldMap);
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
					FieldMapType = kCura.IntegrationPoints.Domain.Models.FieldMapTypeEnum.FolderPathInformation
				}
			};

			// Act
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();
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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().NotBeEmpty()
				.And.Contain(f => f.DestinationField.FieldIdentifier == field2.FieldIdentifier);
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
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().NotBeEmpty()
				.And.Contain(f => f.DestinationField.FieldIdentifier == field2.FieldIdentifier);
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
					FieldMapType = kCura.IntegrationPoints.Domain.Models.FieldMapTypeEnum.Identifier
				},
			};

			// Act
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();
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
					FieldMapType = kCura.IntegrationPoints.Domain.Models.FieldMapTypeEnum.Identifier
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = EmptyField(),
					FieldMapType = kCura.IntegrationPoints.Domain.Models.FieldMapTypeEnum.FolderPathInformation
				}
			};

			// Act
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();
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
					FieldMapType = kCura.IntegrationPoints.Domain.Models.FieldMapTypeEnum.Identifier
				},
				new FieldMap()
				{
					SourceField = FieldConvert.ToFieldEntry(field2),
					DestinationField = FieldConvert.ToFieldEntry(field2),
					FieldMapType = kCura.IntegrationPoints.Domain.Models.FieldMapTypeEnum.FolderPathInformation
				}
			};

			// Act
			var invalidMappedFields = await _sut.ValidateAsync(fieldMap, _SOURCE_WORKSPACE_ID, _DESTINATION_WORKSPACE_ID).ConfigureAwait(false);

			// Assert
			invalidMappedFields.Should().BeEmpty();
		}

		private DocumentFieldInfo CreateWithSampleType(string id)
		{
			return new DocumentFieldInfo(id, $"Field {id}", "Sample Type");
		}

		private DocumentFieldInfo CreateWithType(string id, string type)
		{
			return new DocumentFieldInfo(id, $"Field {id}", type);
		}

		private DocumentFieldInfo CreateWithIdentifier(string id)
		{
			return new DocumentFieldInfo(id, $"Field {id}", "Sample Type")
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
