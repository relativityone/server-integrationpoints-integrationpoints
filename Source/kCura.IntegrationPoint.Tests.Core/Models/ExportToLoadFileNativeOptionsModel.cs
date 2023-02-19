using kCura.IntegrationPoint.Tests.Core.Extensions;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    using System.ComponentModel;

    public class ExportToLoadFileNativeOptionsModel
    {
        [DefaultValue("NATIVE")]
        public string NativeSubdirectoryPrefix { get; set; }

        public ExportToLoadFileNativeOptionsModel()
        {
            this.InitializePropertyDefaultValues();
        }
    }
}
