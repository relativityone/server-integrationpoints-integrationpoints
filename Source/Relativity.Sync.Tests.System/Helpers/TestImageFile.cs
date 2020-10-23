using System;
using System.Data;
using FluentAssertions;
using FluentAssertions.Common;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal class TestImageFile
	{
		public TestImageFile(int documentArtifactId, string identifier, string location, string filename, long size, int? productionId = null)
		{
			DocumentArtifactId = documentArtifactId;
			Location = location;
			Filename = filename;
			Size = size;
			ProductionId = productionId;
			Identifier = identifier;
		}

		public int DocumentArtifactId { get; }
		public string Location { get; }
		public string Filename { get; }
		public long Size { get; }
		public int? ProductionId { get; }
		public string Identifier { get; }

		private static string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private static string _LOCATION_COLUMN_NAME = "Location";
		private static string _FILENAME_COLUMN_NAME = "Filename";
		private static string _SIZE_COLUMN_NAME = "Size";
		private static string IDENTIFIER = "Identifier";


		public static TestImageFile GetImageFile(DataRow dataRow)
		{
			int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
			string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
			string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME);
			long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME);
			string identifier = GetValue<string>(dataRow, IDENTIFIER);

			return new TestImageFile(documentArtifactId, identifier, location, fileName, size);
		}

		private static T GetValue<T>(DataRow row, string columnName)
		{
			object value = null;
			try
			{
				value = row[columnName];
				return (T)value;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error while retrieving image file info from column \"{columnName}\". Value: \"{value}\" Requested type: \"{typeof(T)}\"", ex);
			}
		}

		public static void AssertAreEquivalent(TestImageFile sourceTestImage, TestImageFile destinationTestImage,
			string expectedIdentifier)
		{
			destinationTestImage.Filename.Should().Be(sourceTestImage.Filename);
			destinationTestImage.Identifier.Should().Be(expectedIdentifier);
			destinationTestImage.Size.Should().Be(sourceTestImage.Size);
		}
	}
}
