using System.Threading.Tasks;

namespace Relativity.Sync.Executors.SumReporting
{
    internal interface IJobEndMetricsService
    {
        Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus);
    }
}