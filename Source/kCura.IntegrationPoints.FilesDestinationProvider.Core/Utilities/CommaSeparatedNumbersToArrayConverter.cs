using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Utilities
{
	static class CommaSeparatedNumbersToArrayConverter
	{
		public static int[] ConvertCommaSeparatedStringToArrayOfInts(string commaSeparatedNumbers)
		{
			return commaSeparatedNumbers.Split(',').Select(int.Parse).ToArray();
		}
	}
}
