using kCura.IntegrationPoint.Tests.Core.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    using System.ComponentModel;

    public class ExportToLoadFileVolumeAndSubdirectoryModel
    {
        [DefaultValue("VOL")]
        public string VolumePrefix { get; set; }

        [DefaultValue(1)]
        public int? VolumeStartNumber { get; set; }

        [DefaultValue(2)]
        public int? VolumeNumberOfDigits { get; set; }

        [DefaultValue(4400)]
        public int? VolumeMaxSize { get; set; }

        [DefaultValue(1)]
        public int? SubdirectoryStartNumber { get; set; }

        [DefaultValue(3)]
        public int? SubdirectoryNumberOfDigits { get; set; }

        [DefaultValue(500)]
        public int? SubdirectoryMaxFiles { get; set; }

        public ExportToLoadFileVolumeAndSubdirectoryModel()
        {
            this.InitializePropertyDefaultValues();
        }
    }
}
