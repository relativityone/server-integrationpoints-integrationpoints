using System;

namespace kCura.IntegrationPoints.Web.Metrics
{
    public interface IControllerActionExecutionTimeMetrics
    {
        void LogExecutionTime(string url, DateTime startTime, string method);
    }
}