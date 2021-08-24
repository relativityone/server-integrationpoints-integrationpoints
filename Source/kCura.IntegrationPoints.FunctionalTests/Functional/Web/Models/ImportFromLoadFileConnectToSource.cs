namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    internal class ImportFromLoadFileConnectToSource
    {
        public IntegrationPointImportTypes ImportType { get; set; }

        public string WorkspaceDestinationFolder { get; set; }

        public string ImportSource { get; set; }

        public string Column { get; set; }

        public string Quote { get; set; }
    }
}
