
namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
    internal class ExportToLoadFileConnectToSource
    {
        public int StartExportAtRecord { get; set; }
    }

    internal class ExportToLoadFileConnectToSavedSearchSource : ExportToLoadFileConnectToSource
    {
        public RelativityProviderSources Source { get; } = RelativityProviderSources.SavedSearch;
        public string SavedSearch { get; set; }
    }

}
