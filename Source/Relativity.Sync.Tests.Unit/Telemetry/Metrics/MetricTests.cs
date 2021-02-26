using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	[TestFixture]
	public class MetricTests
	{
		private class TestMetric : MetricBase<TestMetric>
		{
			public int? TestApmMetric { get; set; }

			[APMIgnoreMetric]
			[Metric(MetricType.PointInTimeString, "APM.Ignored.Metric")]
			public string ApmIgnoredMetric { get; set; }
		}
		
		private const string _APPLICATION_NAME = "Relativity.Sync";

		[TestCaseSource(nameof(IMetricImplementersTestCases))]
		public void IMetricImplementingClasses_ShouldHaveOnlyNullableProperties_Guard(Type type)
		{
			// Act
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			// Assert
			properties.Should().OnlyContain(x => GetDefaultValue(x.PropertyType) == null);
		}

		[Test]
		public void GetApmMetrics_ShouldNotCachePropertiesBetweenDifferentMetrics()
		{
			// Arrange
			DocumentBatchEndMetric documentMetric = new DocumentBatchEndMetric()
			{
				BytesTransferred = 100
			};
			ImageBatchEndMetric imageMetric = new ImageBatchEndMetric()
			{
				BytesTransferred = 200
			};

			// Act
			Dictionary<string, object> documentSumMetrics = documentMetric.GetApmMetrics();
			Dictionary<string, object> imageSumMetrics = imageMetric.GetApmMetrics();

			// Assert
			documentSumMetrics.Single(kv => kv.Key == "BytesTransferred")
				.Value.Should().BeAssignableTo<long>().Which.Should().Be(100);
			imageSumMetrics.Single(kv => kv.Key == "BytesTransferred")
				.Value.Should().BeAssignableTo<long>().Which.Should().Be(200);
		}

		[Test]
		public void GetSumMetrics_ShouldIncludeApmIgnoredMetric()
		{
			// Arrange
			TestMetric metric = new TestMetric()
			{
				TestApmMetric = 1,
				ApmIgnoredMetric = "APM Ignore"
			};

			// Act
			List<SumMetric> sumMetrics = metric.GetSumMetrics().ToList();

			// Assert
			sumMetrics.Single().Bucket.Should().Be("APM.Ignored.Metric");
		}

		[Test]
		public void GetSumMetrics_ShouldNotCacheApmIgnoredMetrics()
		{
			// Arrange
			TestMetric metric = new TestMetric()
			{
				TestApmMetric = 1,
				ApmIgnoredMetric = "APM Ignore"
			};

			// Act
			metric.GetApmMetrics();
			List<SumMetric> sumMetrics = metric.GetSumMetrics().ToList();

			// Assert
			sumMetrics.Single().Bucket.Should().Be("APM.Ignored.Metric");
		}

		[Test]
		public void GetApmMetrics_ShouldIgnoreMetric()
		{
			// Arrange
			TestMetric test = new TestMetric()
			{
				TestApmMetric = 1,
				ApmIgnoredMetric = "APM Ignore"
			};

			// Act
			Dictionary<string, object> apmMetrics = test.GetApmMetrics();

			// Assert
			apmMetrics.Keys.Should().NotContain("ApmIgnoredMetric");
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
