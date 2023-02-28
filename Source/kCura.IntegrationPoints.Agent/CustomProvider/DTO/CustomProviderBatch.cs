namespace kCura.IntegrationPoints.Agent.CustomProvider.DTO
{
    public class CustomProviderBatch
    {
        public int BatchID { get; set; }

        public string IDsFilePath { get; set; }

        public string DataFilePath { get; set; }

        public int NumberOfRecordsInDataFile { get; set; }

        public bool IsAddedToImportQueue { get; set; }
    }
}
