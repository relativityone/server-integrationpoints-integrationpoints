namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    using NUnit.Framework;

    public class FolderPathFromFieldStrategy : FolderPathStrategyWithCache
    {
        private readonly string _folderPathFieldName;

        public FolderPathFromFieldStrategy(string folderPathFieldName)
        {
            _folderPathFieldName = folderPathFieldName;
        }

        protected override string GetFolderPathInternal(Document document)
        {
            Assert.That(document[_folderPathFieldName], Is.Not.Null, $"Document {document.ControlNumber} does not have folder path defined");

            return document.ReadAsString(_folderPathFieldName);
        }
    }
}
