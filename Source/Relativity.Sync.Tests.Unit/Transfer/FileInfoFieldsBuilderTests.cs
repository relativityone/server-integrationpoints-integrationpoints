using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public class FileInfoFieldsBuilderTests
	{
		private Mock<INativeFileRepository> _nativeFileRepository;
		private FileInfoFieldsBuilder _instance;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_nativeFileRepository = new Mock<INativeFileRepository>();

			_instance = new FileInfoFieldsBuilder(_nativeFileRepository.Object);
		}

		[Test]
		public void ItShouldReturnBuiltColumns()
		{
			// Arrange
			const int expectedFieldCount = 5;

			// Act
			List<FieldInfoDto> result = _instance.BuildColumns().ToList();

			// Assert
			result.Count.Should().Be(expectedFieldCount);
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.NativeFileLocation).Which.DestinationFieldName.Should().Be("NativeFileLocation");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.NativeFileSize).Which.DestinationFieldName.Should().Be("NativeFileSize");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.NativeFileFilename).Which.DestinationFieldName.Should().Be("NativeFileFilename");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.RelativityNativeType).Which.SourceFieldName.Should().Be("RelativityNativeType");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.RelativityNativeType).Which.DestinationFieldName.Should().Be("RelativityNativeType");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.SupportedByViewer).Which.SourceFieldName.Should().Be("SupportedByViewer");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.SupportedByViewer).Which.DestinationFieldName.Should().Be("SupportedByViewer");
		}

		[Test]
		public async Task ItShouldReturnFileInfoRowValuesBuilder()
		{
			// Act
			ISpecialFieldRowValuesBuilder result = await _instance.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>()).ConfigureAwait(false);

			// Assert
			result.Should().BeOfType<FileInfoRowValuesBuilder>();
			_nativeFileRepository.Verify(r => r.QueryAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<int>>()), Times.Once);
		}
	}
}
