using System.Linq;
using System.Reflection;
using Autofac;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Registers components related to Sync telemetry.
	/// </summary>
	internal sealed class TelemetryInstaller : IInstaller
	{
		/// <inheritdoc />
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<SystemStopwatch>().As<IStopwatch>();
			builder.RegisterType<SyncMetrics>().As<ISyncMetrics>();
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<ISyncMetricsSink>())
				.ToArray()).As<ISyncMetricsSink>();
		}
	}
}