using System.Text;

namespace kCura.IntegrationPoints.Data.Extensions
{
    public static class JobHistoryExtensions
    {
        public static string Stringify(this JobHistory jobHistory)
        {
            if(jobHistory == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("JobistoryDetails:");
            sb.AppendLine($"ArtifactId: {jobHistory.ArtifactId}");
            sb.AppendLine($"BatchInstance: {jobHistory.BatchInstance}");
            sb.AppendLine($"JobId: {jobHistory.JobID}");
            sb.AppendLine($"JobStatus: {jobHistory.JobStatus?.Name}");

            return sb.ToString();
        }
    }
}
