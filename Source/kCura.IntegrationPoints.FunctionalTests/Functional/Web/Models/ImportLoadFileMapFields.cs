using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    internal class ImportLoadFileMapFields
    {
        public RelativityProviderOverwrite Overwrite { get; set; }

        public RelativityProviderCopyNativeFiles CopyNativeFiles { get; set; }

        public YesNo UseFolderPathInformation { get; set; } 

        public string NativeFilePath { get; set; }

        public string FolderPathInformation { get; set; }
    }
}
