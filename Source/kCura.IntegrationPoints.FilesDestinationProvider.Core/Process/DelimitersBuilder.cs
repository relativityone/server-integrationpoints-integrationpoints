using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    internal class DelimitersBuilder : IDelimitersBuilder
    {
        private static readonly char _concordanceColumn = Convert.ToChar(20);
        private static readonly char _concordanceQuote = Convert.ToChar(254);
        private static readonly char _concordanceNewline = Convert.ToChar(174);
        private static readonly char _csvColumn = Convert.ToChar(44);
        private static readonly char _csvQuote = Convert.ToChar(34);
        private static readonly char _csvNewline = Convert.ToChar(10);
        private static readonly char _multivalue = Convert.ToChar(59);
        private static readonly char _nestedvalue = Convert.ToChar(92);

        public void SetDelimiters(ExportFile exportFile, ExportSettings exportSettings)
        {
            switch (exportSettings.OutputDataFileFormat)
            {
                case ExportSettings.DataFileFormat.CSV:
                    SetCsvDelimiters(exportFile);
                    return;
                case ExportSettings.DataFileFormat.Concordance:
                    SetConcordanceDelimiters(exportFile);
                    return;
                case ExportSettings.DataFileFormat.Custom:
                    SetCustomDelimiters(exportFile, exportSettings);
                    return;
            }
        }

        private static void SetCsvDelimiters(ExportFile exportFile)
        {
            exportFile.RecordDelimiter = _csvColumn;
            exportFile.QuoteDelimiter = _csvQuote;
            exportFile.NewlineDelimiter = _csvNewline;
            exportFile.MultiRecordDelimiter = _multivalue;
            exportFile.NestedValueDelimiter = _nestedvalue;
        }

        private static void SetConcordanceDelimiters(ExportFile exportFile)
        {
            exportFile.RecordDelimiter = _concordanceColumn;
            exportFile.QuoteDelimiter = _concordanceQuote;
            exportFile.NewlineDelimiter = _concordanceNewline;
            exportFile.MultiRecordDelimiter = _multivalue;
            exportFile.NestedValueDelimiter = _nestedvalue;
        }

        private static void SetCustomDelimiters(ExportFile exportFile, ExportSettings exportSettings)
        {
            exportFile.RecordDelimiter = exportSettings.ColumnSeparator;
            exportFile.QuoteDelimiter = exportSettings.QuoteSeparator;
            exportFile.NewlineDelimiter = exportSettings.NewlineSeparator;
            exportFile.MultiRecordDelimiter = exportSettings.MultiValueSeparator;
            exportFile.NestedValueDelimiter = exportSettings.NestedValueSeparator;
        }
    }
}
