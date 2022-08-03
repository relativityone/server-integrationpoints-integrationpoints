namespace kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile
{
    public static class ExportToLoadFileSourceConstants
    {
        public const string SAVED_SEARCH = "Saved Search";
        public const string PRODUCTION = "Production";
        public const string FOLDER = "Folder";
        public const string FOLDER_AND_SUBFOLDERS = "Folder + Subfolders";

        public static bool IsFolder(string source)
        {
            return source.Equals(FOLDER) || source.Equals(FOLDER_AND_SUBFOLDERS);
        }
    }
}
