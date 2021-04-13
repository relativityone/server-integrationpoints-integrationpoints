namespace Relativity.Sync.Executors.SumReporting
{
	internal interface IJobEndMetricsServiceFactory
	{
		IJobEndMetricsService CreateJobEndMetricsService(bool isSuspended);
	}
}
