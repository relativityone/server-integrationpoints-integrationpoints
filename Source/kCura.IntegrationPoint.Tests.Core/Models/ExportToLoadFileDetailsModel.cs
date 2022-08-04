using kCura.IntegrationPoint.Tests.Core.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    using System.ComponentModel;

    public class ExportToLoadFileDetailsModel
    {
        [DefaultValue(true)]
        public bool? LoadFile { get; set; }

        [DefaultValue(false)]
        public bool? ExportImages { get; set; }

        [DefaultValue(false)]
        public bool? ExportNatives { get; set; }

        [DefaultValue(false)]
        public bool? ExportTextFieldsAsFiles { get; set; }

        public ExportToLoadFileProviderModel.DestinationFolderTypeEnum? DestinationFolder { get; set; }

        [DefaultValue(true)]
        public bool? CreateExportFolder { get; set; }

        [DefaultValue(false)]
        public bool? OverwriteFiles { get; set; }

        public ExportToLoadFileDetailsModel()
        {
            this.InitializePropertyDefaultValues();
        }
    }
}