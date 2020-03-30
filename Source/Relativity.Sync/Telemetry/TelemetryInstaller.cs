﻿using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Relativity.API;

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
			builder.RegisterType<APMClient>().As<IAPMClient>();
			builder.RegisterType<SystemStopwatch>().As<IStopwatch>();
			builder.RegisterType<SyncMetrics>().As<ISyncMetrics>();
			builder.RegisterType<JobStatisticsContainer>().As<IJobStatisticsContainer>().SingleInstance();
			builder.Register(c => EnvironmentPropertyProvider.Create(c.Resolve<ISyncServiceManager>(), c.Resolve<ISyncLog>()))
				.As<IEnvironmentPropertyProvider>()
				.SingleInstance();
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<ISyncMetricsSink>())
				.ToArray()).As<ISyncMetricsSink>();
			builder.Register(c => new Lazy<ISyncMetrics>(c.Resolve<ISyncMetrics>));
		}
	}
}