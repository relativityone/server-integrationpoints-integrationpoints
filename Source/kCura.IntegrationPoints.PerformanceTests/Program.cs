using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using kCura.IntegrationPoints.PerformanceTests.TestCases;

namespace kCura.IntegrationPoints.PerformanceTests
{
	public static class Program
	{
		static void Main()
		{
			var summary = BenchmarkRunner.Run<FieldsAccessIapiObjectManager>(new AllowNonOptimized());

			Console.ReadKey();
		}

		public class AllowNonOptimized : ManualConfig
		{
			public AllowNonOptimized()
			{
				Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS

				Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
				Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
				Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
			}
		}
	}
}
