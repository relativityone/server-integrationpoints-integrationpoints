using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
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
			const int expectedFieldCount = 5;

			List<FieldInfoDto> result = _instance.BuildColumns().ToList();

			result.Count.Should().Be(expectedFieldCount);
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.NativeFileLocation).Which.DisplayName.Should().Be("NativeFileLocation");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.NativeFileSize).Which.DisplayName.Should().Be("NativeFileSize");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.NativeFileFilename).Which.DisplayName.Should().Be("NativeFileFilename");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.RelativityNativeType).Which.DisplayName.Should().Be("RelativityNativeType");
			result.Should().Contain(info => info.SpecialFieldType == SpecialFieldType.SupportedByViewer).Which.DisplayName.Should().Be("SupportedByViewer");
		}

		[Test]
		public async Task ItShouldReturnFileInfoRowValuesBuilder()
		{
			ISpecialFieldRowValuesBuilder result = await _instance.GetRowValuesBuilderAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, Array.Empty<int>()).ConfigureAwait(false);

			result.Should().BeOfType<FileInfoRowValuesBuilder>();
			_nativeFileRepository.Verify(r => r.QueryAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<int>>()), Times.Once);
		}
	}
}
