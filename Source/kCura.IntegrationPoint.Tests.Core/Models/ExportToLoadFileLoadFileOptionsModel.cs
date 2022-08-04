using kCura.IntegrationPoint.Tests.Core.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    using System.ComponentModel;

    public class ExportToLoadFileLoadFileOptionsModel
    {
        [DefaultValue("No Image Load File")]
        public string ImageFileFormat { get; set; }

        [DefaultValue("Relativity (.dat)")]
        public string DataFileFormat { get; set; }

        [DefaultValue("Unicode")]
        public string DataFileEncoding { get; set; }

        [DefaultValue(ExportToLoadFileProviderModel.FilePathTypeEnum.Relative)]
        public ExportToLoadFileProviderModel.FilePathTypeEnum FilePathType { get; set; }

        [DefaultValue("")]
        public string UserPrefix { get; set; }

        [DefaultValue(false)]
        public bool? IncludeNativeFilesPath { get; set; }

        [DefaultValue(false)]
        public bool? ExportMultiChoiceAsNested { get; set; }

        [DefaultValue("Identifier")]
        public string NameOutputFilesAfter { get; set; }

        [DefaultValue(false)]
        public bool? AppendOriginalFileName { get; set; }

        public ExportToLoadFileLoadFileOptionsModel()
        {
            this.InitializePropertyDefaultValues();
        }
    }
}