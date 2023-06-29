using System;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Exceptions
{
    public class KubernetesException : Exception
    {
        public KubernetesException(string message)
            : base(message)
        {
        }

        public static KubernetesException CreateTransientJobException(Job job)
            => new KubernetesException($"Job {job.JobId} failed because Job Agent stopped due to underlying system layer error and job was left in unknown state. Please try to run this job again.");
    }
}
