using System.Data;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers.Converters
{
	internal static class DataRowExtensions
	{
		private static string _IDENTIFIER_COLUMN_NAME = "Identifier";
		private const string _IN_REPOSITORY_COLUMN_NAME = "InRepository";
		private const string _LOCATION_COLUMN_NAME = "Location";
		private const string _FILENAME_COLUMN_NAME = "Filename";

		public static FileTestDto ToFileTestDto(this DataRow row)
		{
			if (row == null)
			{
				return null;
			}

			var filename = (string)row[_FILENAME_COLUMN_NAME];
			var location = (string)row[_LOCATION_COLUMN_NAME];
			var inRepository = (bool)row[_IN_REPOSITORY_COLUMN_NAME];
			var identifier = (string) row[_IDENTIFIER_COLUMN_NAME];

			return new FileTestDto(
				filename,
				location,
				identifier,
				inRepository);
		}
	}
}
