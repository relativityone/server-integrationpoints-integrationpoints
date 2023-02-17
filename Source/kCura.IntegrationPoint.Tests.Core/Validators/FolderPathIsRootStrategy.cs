namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class FolderPathIsRootStrategy : FolderPathStrategyWithCache
    {
        private readonly string _folderName;

        public FolderPathIsRootStrategy(string folderName = "")
        {
            _folderName = folderName;
        }

        protected override string GetFolderPathInternal(Document document)
        {
            return _folderName;
        }
    }
}
