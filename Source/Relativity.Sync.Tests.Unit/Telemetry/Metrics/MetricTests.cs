using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	[TestFixture]
	public class MetricTests
	{
		private const string _APPLICATION_NAME = "Relativity.Sync";

		[TestCaseSource(nameof(IMetricImplementersTestCases))]
		public void IMetricImplementingClasses_ShouldHaveOnlyNullableProperties_Guard(Type type)
		{
			// Act
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			// Assert
			properties.Should().OnlyContain(x => GetDefaultValue(x.PropertyType) == null);
		}

		public static object GetDefaultValue(Type t)
		{
			if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
				return Activator.CreateInstance(t);

			return null;
		}

		public static IEnumerable<Type> IMetricImplementersTestCases =>
			AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => x.GetName().Name.StartsWith(_APPLICATION_NAME))
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(IMetric).IsAssignableFrom(p)).ToList();
	}
}
