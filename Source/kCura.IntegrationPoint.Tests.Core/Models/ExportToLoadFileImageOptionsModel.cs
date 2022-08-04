using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    using System.ComponentModel;

    public class ExportToLoadFileImageOptionsModel
    {
        [DefaultValue("Single page TIFF/JPEG")]
        public string ImageFileType { get; set; }
        
        public ImagePrecedence ImagePrecedence { get; set; }

        [DefaultValue("IMG")]
        public string ImageSubdirectoryPrefix { get; set; }

        public ExportToLoadFileImageOptionsModel()
        {
            this.InitializePropertyDefaultValues();
        }
    }
}