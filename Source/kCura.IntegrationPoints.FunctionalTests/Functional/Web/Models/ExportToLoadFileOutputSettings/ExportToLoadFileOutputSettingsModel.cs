namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings
{
    internal class ExportToLoadFileOutputSettingsModel
    {
	    public ImageFileFormats ImageFileFormat { get; set; } = ImageFileFormats.Opticon;
	    public DataFileFormats DataFileFormat { get; set; } = DataFileFormats.Relativity;
	    public NameOutputFilesAfterOptions NameOutputFilesAfter { get; set; } = NameOutputFilesAfterOptions.Identifier;

        public ImageFileTypes FileType { get; set; }
        public ImagePrecedences ImagePrecedence { get; set; }
        public string SubdirectoryImagePrefix { get; set; }

        public string SubdirectoryNativePrefix { get; set; }
    }
}
