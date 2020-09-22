using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	/// <summary>
	/// ImageBatchDataReader inherits most of the functionality from base class
	/// which is already covered by unit tests - see <see cref="SourceWorkspaceDataReaderTests"/>
	/// </summary>
	[TestFixture]
	public class ImageBatchDataReaderTests
	{
		const string IdentifierFieldName = "IdentifierField";
		private const int SourceWorkspaceId = 11;

		private FieldInfoDto _identifierField;

		[SetUp]
		public void SetUp()
		{
			_identifierField = FieldInfoDto.DocumentField(IdentifierFieldName, IdentifierFieldName, true);
			_identifierField.DocumentFieldIndex = 0;
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(4)]
		public void Read_ShouldReturnRowForEachImageInSingleDocument(int numberOfImagesInDocument)
		{
			// Arrange
			const int numberOfDocuments = 1;

			Mock<IImageSpecialFieldRowValuesBuilder> imageRowValuesBuilderMock = new Mock<IImageSpecialFieldRowValuesBuilder>();
			imageRowValuesBuilderMock
				.Setup(x => x.BuildRowsValues(It.IsAny<FieldInfoDto>(), It.IsAny<RelativityObjectSlim>()))
				.Returns(Enumerable.Range(0, numberOfImagesInDocument).Select(i => $"image-{i}"));

			Mock<IFieldManager> fieldManager = new Mock<IFieldManager>();
			fieldManager.Setup(x => x.GetObjectIdentifierFieldAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_identifierField);
			fieldManager.Setup(x => x.CreateImageSpecialFieldRowValueBuildersAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(new Dictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder>()
				{
					{
						SpecialFieldType.ImageFileName, imageRowValuesBuilderMock.Object
					}
				});

			Mock<IExportDataSanitizer> exportDataSanitizer = new Mock<IExportDataSanitizer>();
			IReadOnlyList<FieldInfoDto> allFields = new List<FieldInfoDto>()
			{
				FieldInfoDto.ImageFileNameField(),
				_identifierField
			};

			Action<string, string> itemLevelErrorHandler = (s1, s2) => { };

			ImageBatchDataReader sut = new ImageBatchDataReader(
				CreateTemplateDataTable(allFields),
				SourceWorkspaceId,
				GenerateBatch(numberOfDocuments),
				allFields,
				fieldManager.Object,
				exportDataSanitizer.Object,
				itemLevelErrorHandler,
				CancellationToken.None);

			// Act & Assert
			bool read;
			for (int i = 0; i < numberOfImagesInDocument; i++)
			{
				read = sut.Read();
				read.Should().BeTrue();

				sut.GetValue(0).Should().BeOfType<string>().Which.Should().Be($"image-{i}");
			}

			read = sut.Read();
			read.Should().BeFalse();
		}

		private static DataTable CreateTemplateDataTable(IEnumerable<FieldInfoDto> fields)
		{
			var dataTable = new DataTable();
			DataColumn[] columns = fields
				.Select(field => new DataColumn(field.DestinationFieldName, typeof(string)))
				.ToArray();
			dataTable.Columns.AddRange(columns);
			return dataTable;
		}
		
		private static RelativityObjectSlim[] GenerateBatch(int size, int numValues = 1)
		{
			return Enumerable.Range(0, size)
				.Select(_ => GenerateObject(numValues))
				.ToArray();
		}

		private static RelativityObjectSlim GenerateObject(int numValues)
		{
			var obj = new RelativityObjectSlim
			{
				ArtifactID = Guid.NewGuid().GetHashCode(),
				Values = Enumerable.Range(0, numValues).Select(_ => new object()).ToList()
			};
			return obj;
		}

	}
}