using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Logging;
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
				.Setup(x => x.BuildRowsValues(It.Is<FieldInfoDto>(field => field.SpecialFieldType == SpecialFieldType.ImageFileName), It.IsAny<RelativityObjectSlim>(), It.IsAny<Func<RelativityObjectSlim, string>>()))
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
				0,
				CancellationToken.None,
				new EmptyLogger());

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

		[Test]
		public void Read_ShouldReportItemLevelError_WhenSpecialFieldsBuildersReturnsDifferentNumberOfItems()
		{
			// Arrange
			const int numberOfDocuments = 1;

			Mock<IImageSpecialFieldRowValuesBuilder> imageRowValuesBuilderMock = new Mock<IImageSpecialFieldRowValuesBuilder>();
			const int imageFileNameCount = 5;
			imageRowValuesBuilderMock
				.Setup(x => x.BuildRowsValues(It.Is<FieldInfoDto>(field => field.SpecialFieldType == SpecialFieldType.ImageFileName), It.IsAny<RelativityObjectSlim>(), It.IsAny<Func<RelativityObjectSlim, string>>()))
				.Returns(Enumerable.Range(0, imageFileNameCount).Select(i => $"image-{i}"));
			const int imageFileLocationCount = 2;
			imageRowValuesBuilderMock
				.Setup(x => x.BuildRowsValues(It.Is<FieldInfoDto>(field => field.SpecialFieldType == SpecialFieldType.ImageFileLocation), It.IsAny<RelativityObjectSlim>(), It.IsAny<Func<RelativityObjectSlim, string>>()))
				.Returns(Enumerable.Range(0, imageFileLocationCount).Select(i => $"image-{i}"));

			Mock<IFieldManager> fieldManager = new Mock<IFieldManager>();
			fieldManager.Setup(x => x.GetObjectIdentifierFieldAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_identifierField);
			fieldManager.Setup(x => x.CreateImageSpecialFieldRowValueBuildersAsync(It.IsAny<int>(), It.IsAny<int[]>()))
				.ReturnsAsync(new Dictionary<SpecialFieldType, IImageSpecialFieldRowValuesBuilder>()
				{
					{
						SpecialFieldType.ImageFileName, imageRowValuesBuilderMock.Object
					},
					{
						SpecialFieldType.ImageFileLocation, imageRowValuesBuilderMock.Object
					}
				});

			Mock<IExportDataSanitizer> exportDataSanitizer = new Mock<IExportDataSanitizer>();
			IReadOnlyList<FieldInfoDto> allFields = new List<FieldInfoDto>()
			{
				FieldInfoDto.ImageFileNameField(),
				FieldInfoDto.ImageFileLocationField(),
				_identifierField
			};

			Mock<Action<string, string>> itemLevelErrorHandlerMock = new Mock<Action<string, string>>();

			ImageBatchDataReader sut = new ImageBatchDataReader(
				CreateTemplateDataTable(allFields),
				SourceWorkspaceId,
				GenerateBatch(numberOfDocuments),
				allFields,
				fieldManager.Object,
				exportDataSanitizer.Object,
				itemLevelErrorHandlerMock.Object,
				0,
				CancellationToken.None,
				new EmptyLogger());

			// Act
			bool read = sut.Read();

			// Assert
			read.Should().BeFalse();
			itemLevelErrorHandlerMock.Verify(x => x.Invoke(It.IsAny<string>(), It.Is<string>(message => 
				message.Contains($"Special fields builders should all return equal number of field values, but was: ImageFileName ({imageFileNameCount}),ImageFileLocation ({imageFileLocationCount})"))),
				Times.Exactly(numberOfDocuments));
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