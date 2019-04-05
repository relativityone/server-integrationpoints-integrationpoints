using FluentAssertions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Utilities;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Utilities
{
	[TestFixture]
	class CommaSeparatedNumbersToArrayConverterTests
	{
		[Test]
		public void ShouldParseCorrectData() //todo: change name to something more better :_)
		{
			//arrange
			string data = "123,321,1,9999";
			//act
			int[] parsed = CommaSeparatedNumbersToArrayConverter.ConvertToArray(data);
			//assert
			parsed.Should().BeSameAs(new int[]{ 123, 321, 1, 9999 });
		}
	}
}
