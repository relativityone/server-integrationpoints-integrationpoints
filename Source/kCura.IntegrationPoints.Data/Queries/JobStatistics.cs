using System.Data;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class JobStatistics
    {
        public int Completed { get; set; }
        
        /// <summary>
        /// This is a value from JobReport from OnComplete IAPI event
        /// </summary>
        public int Errored { get; set; }
        
        /// <summary>
        /// This is a count of OnError invocations
        /// </summary>
        public int ImportApiErrors { get; set; }
        public int Imported { get { return Completed - ImportApiErrors; } }

        public static JobStatistics Populate(DataRow row)
        {
            var s = new JobStatistics();
            s.Completed = row.Field<int>("TotalRecords");
            s.Errored = row.Field<int>("ErrorRecords");
            s.ImportApiErrors = row.Field<int>("ImportApiErrors");
            return s;
        }
    }
}