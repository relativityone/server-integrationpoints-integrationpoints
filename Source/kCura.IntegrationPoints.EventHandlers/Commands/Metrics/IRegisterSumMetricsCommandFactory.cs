namespace kCura.IntegrationPoints.EventHandlers.Commands.Metrics
{
	public interface IRegisterSumMetricsCommandFactory
	{
		IEHCommand CreateCommand<T>() where T : IEHCommand;
	}
}
