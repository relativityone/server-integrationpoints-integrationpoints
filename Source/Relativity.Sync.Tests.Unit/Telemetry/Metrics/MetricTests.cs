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
        private const string _APPLICATION_TESTS_NAME = "Relativity.Sync.Tests";

        [TestCaseSource(nameof(IMetricImplementersTestCases))]
        public void IMetricImplementingClasses_ShouldHaveOnlyNullableProperties_Guard(Type type)
        {
            // Act
            PropertyInfo[] properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType.IsClass)
                .ToArray();

            // Assert
            properties.Should().OnlyContain(x => GetDefaultValue(x.PropertyType) == null, $"metric of type '{type.FullName}' should have only nullable properties");
        }

        [Test]
        public void MetricBaseImplementingClasses_ShouldHaveUniqueGenericArgumentType_Guard()
        {
            // Act
            List<Type> metricBaseImplementersGenericArgumentTypes = MetricBaseImplementersTestCases
                .Select(t => t.BaseType.GetGenericArguments()[0]).ToList();

            // Assert
            metricBaseImplementersGenericArgumentTypes.Should().OnlyHaveUniqueItems();
        }

        [TestCaseSource(nameof(MetricBaseImplementersTestCases))]
        public void MetricBaseImplementingClasses_ShouldBeSealed_Guard(Type type)
        {
            // Assert
            type.IsSealed.Should().BeTrue();
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

        [Test]
        public void GetApmMetrics_ShouldIncludeCommonApmMetricsCustomData()
        {
            // Arrange
            TestMetric metric = new TestMetric()
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ExecutingApplication = "app",
                ExecutingApplicationVersion = "1.0",
                DataSourceType = "data source",
                DataDestinationType = "data dest",
                FlowName = "flow type",
                IsRetry = true
            };

            // Act
            Dictionary<string, object> apmMetrics = metric.GetApmMetrics();

            // Assert
            apmMetrics[nameof(IMetric.CorrelationId)].Should().Be(metric.CorrelationId);
            apmMetrics[nameof(IMetric.ExecutingApplication)].Should().Be(metric.ExecutingApplication);
            apmMetrics[nameof(IMetric.ExecutingApplicationVersion)].Should().Be(metric.ExecutingApplicationVersion);
            apmMetrics[nameof(IMetric.DataSourceType)].Should().Be(metric.DataSourceType);
            apmMetrics[nameof(IMetric.DataDestinationType)].Should().Be(metric.DataDestinationType);
            apmMetrics[nameof(IMetric.FlowName)].Should().Be(metric.FlowName);
            apmMetrics[nameof(IMetric.IsRetry)].Should().Be(metric.IsRetry);
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

        public static IEnumerable<Type> MetricBaseImplementersTestCases =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith(_APPLICATION_NAME) && !a.GetName().Name.StartsWith(_APPLICATION_TESTS_NAME))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsGenericType && t.BaseType != null && t.BaseType.IsGenericType && (t.BaseType.GetGenericTypeDefinition() == typeof(MetricBase<>) || t.BaseType.GetGenericTypeDefinition() == typeof(BatchEndMetric<>)));
    }
}
