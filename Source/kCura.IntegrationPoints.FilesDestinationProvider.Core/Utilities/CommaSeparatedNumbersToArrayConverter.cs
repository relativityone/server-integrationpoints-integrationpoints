using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Utilities
{
    public static class CommaSeparatedNumbersToArrayConverter
    {
        public static int[] Convert(string commaSeparatedNumbers)
        {
            return commaSeparatedNumbers
                .Split(',')
                .Select(int.Parse)
                .ToArray();
        }
    }
}
