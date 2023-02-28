using System;

namespace kCura.IntegrationPoints.Data.DTO
{
    public class ExportInitializationResultsDto
    {
        public ExportInitializationResultsDto(Guid runID, long recordCount, string[] fieldNames)
        {
            RunID = runID;
            RecordCount = recordCount;
            FieldNames = fieldNames;
        }

        public Guid RunID { get; }

        public long RecordCount { get; }

        public string[] FieldNames { get; }
    }
}
