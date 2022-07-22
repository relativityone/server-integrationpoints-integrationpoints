using System;
using System.Data;
using FluentAssertions;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    internal class TestImageFile
    {
        public TestImageFile(int documentArtifactId, string identifier, string location, string filename, long size, int order, int? productionId = null)
        {
            DocumentArtifactId = documentArtifactId;
            Location = location;
            Filename = filename;
            Size = size;
            Order = order;
            ProductionId = productionId;
            Identifier = identifier;
        }

        public int DocumentArtifactId { get; }
        public string Location { get; }
        public string Filename { get; }
        public long Size { get; }
        public int Order { get; }
        public int? ProductionId { get; }
        public string Identifier { get; }

        private static string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
        private static string _LOCATION_COLUMN_NAME = "Location";
        private static string _FILENAME_COLUMN_NAME = "Filename";
        private static string _SIZE_COLUMN_NAME = "Size";
        private static string _IDENTIFIER = "Identifier";
        private static string _ORDER = "Order";

        public static TestImageFile GetFile(DataRow dataRow)
        {
            int documentArtifactId = GetValue<int>(dataRow, _DOCUMENT_ARTIFACT_ID_COLUMN_NAME);
            string location = GetValue<string>(dataRow, _LOCATION_COLUMN_NAME);
            string fileName = GetValue<string>(dataRow, _FILENAME_COLUMN_NAME);
            long size = GetValue<long>(dataRow, _SIZE_COLUMN_NAME);
            string identifier = GetValue<string>(dataRow, _IDENTIFIER);

            int order = GetValue<int>(dataRow, _ORDER);
            
            return new TestImageFile(documentArtifactId, identifier, location, fileName, size, order);
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

        public static void AssertAreEquivalent(TestImageFile sourceTestImage, TestImageFile destinationTestImage, string expectedIdentifier)
        {
            destinationTestImage.Filename.Should().Be(sourceTestImage.Filename);
            destinationTestImage.Identifier.Should().Be(expectedIdentifier);
            destinationTestImage.Size.Should().Be(sourceTestImage.Size);
            destinationTestImage.Order.Should().Be(sourceTestImage.Order);
        }

        /// <summary>
        /// Expects that source and destination image point to the same location.
        /// </summary>
        public static void AssertImageIsLinked(TestImageFile source, TestImageFile destination)
        {
            destination.Location.Should().Be(source.Location);
        }
    }
}
