using kCura.IntegrationPoints.FtpProvider.Helpers;
using Microsoft.VisualBasic.FileIO;

namespace kCura.IntegrationPoints.FtpProvider.Parser
{
    public class ParserOptions
    {
        /// <summary>
        /// Indicates whether the file to be parsed is delimited or fixed-width.
        /// </summary>
        public FieldType TextFieldType { get; set; }

        /// <summary>
        /// Defines the delimiters for a text file.
        /// </summary>
        public string[] Delimiters { get; set; }

        /// <summary>
        /// Denotes whether fields are enclosed in quotation marks when a delimited file is being parsed.
        /// </summary>
        public bool HasFieldsEnclosedInQuotes { get; set; }

        /// <summary>
        /// Indicates whether leading and trailing white space should be trimmed from field values.
        /// </summary>
        public bool TrimWhiteSpace { get; set; }

        /// <summary>
        /// Indicates whether first line contains column names.
        /// </summary>
        public bool FirstLineContainsColumnNames { get; set; }

        /// <summary>
        /// Sets the line in order to properly indicate line number in text file
        /// </summary>
        public long FirstLineNumber { get; set; }

        public static ParserOptions GetDefaultParserOptions()
        {
            return new ParserOptions
            {
                TextFieldType = FieldType.Delimited,
                Delimiters = new[] { Constants.Delimiter },
                HasFieldsEnclosedInQuotes = true,
                TrimWhiteSpace = true,
                FirstLineContainsColumnNames = true,
                FirstLineNumber = 0
            };
        }
    }
}
