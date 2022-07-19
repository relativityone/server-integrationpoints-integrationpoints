namespace Relativity.Sync.WorkspaceGenerator.Import
{
    public class ImageFileDTO
    {
        public ImageFileDTO(string documentControlNumber, string imageFilePath, string imageFileName, string begBates)
        {
            DocumentControlNumber = documentControlNumber;
            ImageFilePath = imageFilePath;
            ImageFileName = imageFileName;
            BegBates = begBates;
        }

        public string DocumentControlNumber { get; }
        public string ImageFileName { get; }
        public string ImageFilePath { get; }
        public string BegBates { get; }
    }
}