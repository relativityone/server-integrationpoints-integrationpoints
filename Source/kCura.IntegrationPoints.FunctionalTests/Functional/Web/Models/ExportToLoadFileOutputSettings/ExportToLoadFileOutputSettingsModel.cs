using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings
{
    internal class ExportToLoadFileOutputSettingsModel
    {
        public ImageFileFormats ImageFileFormat { get; set; }
        public DataFileFormats DataFileFormat { get; set; }
        public NameOutputFilesAfterOptions NameOutputFilesAfter { get; set; }

        public ImageFileTypes FileType { get; set; }
        public ImagePrecedences ImagePrecedence { get; set; }
    }
}
