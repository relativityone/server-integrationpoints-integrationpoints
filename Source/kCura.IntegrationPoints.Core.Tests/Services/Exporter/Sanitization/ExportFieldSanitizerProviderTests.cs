using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
	public class ExportFieldSanitizerProviderTests
	{
		private Mock<ISerializer> _serializerMock;
		private Mock<IChoiceCache> _choiceCacheMock;
		private Mock<IChoiceTreeToStringConverter> _choiceTreeConverterMock;

		private ExportFieldSanitizerProvider _sut;

		[SetUp]
		public void SetUp()
		{
			_serializerMock = new Mock<ISerializer>();
			_choiceCacheMock = new Mock<IChoiceCache>();
			_choiceTreeConverterMock = new Mock<IChoiceTreeToStringConverter>();
			_sut = new ExportFieldSanitizerProvider(
				_serializerMock.Object, 
				_choiceCacheMock.Object,
				_choiceTreeConverterMock.Object);
		}

		[Test]
		public void GetExportFieldSanitizers_ReturnsProperListOfSanitizers()
		{
			// arrange
			const int expectedNumberOfSaniziters = 4;
			IEnumerable<Type> expectedTypes = new List<Type>
			{
				typeof(SingleObjectFieldSanitizer),
				typeof(MultipleObjectFieldSanitizer),
				typeof(SingleChoiceFieldSanitizer),
				typeof(MultipleChoiceFieldSanitizer)
			};

			// act
			IList<IExportFieldSanitizer> result = _sut.GetExportFieldSanitizers();

			// assert
			result.Count.Should().Be(expectedNumberOfSaniziters);
			foreach (var expectedType in expectedTypes)
			{
				result.Should().Contain(x => x.GetType() == expectedType);
			}
		}
	}
}
