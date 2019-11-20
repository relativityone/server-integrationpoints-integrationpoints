using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	/// Simple data bag representing a metric sent to a sink.
	/// </summary>
	internal sealed class Metric
	{
		private IDictionary<string, object> _customData;

		private const string _SYNC_APPLICATION_NAME = "Relativity.Sync";

		private static readonly IEnumerable<PropertyInfo> PublicInstanceProperties =
			typeof(Metric)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null);

		// Info for public properties with get methods on this class.
		// These are set by compile time, so we can calculate these ahead of time.
		private static readonly IEnumerable<PropertyInfo> PublicInstanceValueProperties =
			PublicInstanceProperties
				.Where(p => p.PropertyType != typeof(IDictionary<string, object>));

		private static readonly IEnumerable<PropertyInfo> PublicInstanceDictionaryProperties =
			PublicInstanceProperties
				.Where(p => p.PropertyType == typeof(IDictionary<string, object>));

		private Metric(string name, MetricType type, string workflowId)
		{
			Name = name;
			Type = type;
			WorkflowId = workflowId;
		}

		/// <summary>
		/// Name of the application.
		/// </summary>
		/// <remarks>
		/// Set to <see cref="_SYNC_APPLICATION_NAME"/> to easily distinguish between Sync and RIP
		/// </remarks>
		public string Application { get; } = _SYNC_APPLICATION_NAME;

		/// <summary>
		/// Name of this metric.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Type of metric this represents, e.g. a timed operation, a counter, etc.
		/// </summary>
		public MetricType Type { get; }

		/// <summary>
		/// ID that correlates the metric to a particular system usage workflow.
		/// </summary>
		public string WorkflowId { get; set; }

		/// <summary>
		/// Value of this metric.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Status of the operation related to this metric.
		/// </summary>
		public ExecutionStatus ExecutionStatus { get; set; }

		/// <summary>
		/// Any custom data associated with this metric.
		/// </summary>
		public IDictionary<string, object> CustomData
		{
			get => _customData ?? (_customData = new Dictionary<string, object>());

			set => _customData = value;
		}

		/// <summary>
		/// Creates a <see cref="Metric" /> representing the result of a timed operation.
		/// </summary>
		/// <param name="name">Name or bucket for the metric</param>
		/// <param name="duration">Duration of the operation</param>
		/// <param name="executionStatus">Result of the operation</param>
		/// <param name="workflowId">The ID which correlates the metric to a particular workflow.</param>
		/// <returns></returns>
		public static Metric TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus, string workflowId)
		{
			return new Metric(name, MetricType.TimedOperation, workflowId)
			{
				Value = duration.TotalMilliseconds,
				ExecutionStatus = executionStatus
			};
		}

		/// <summary>
		/// Creates a <see cref="Metric" /> representing the result of a counter operation.
		/// </summary>
		/// <param name="name">Name or bucket for the metric</param>
		/// <param name="executionStatus">Result of the operation</param>
		/// <param name="workflowId">The ID which correlates the metric to a particular workflow.</param>
		/// <returns></returns>
		public static Metric CountOperation(string name, ExecutionStatus executionStatus, string workflowId)
		{
			return new Metric(name, MetricType.Counter, workflowId)
			{
				ExecutionStatus = executionStatus
			};
		}

		/// <summary>
		/// Creates a <see cref="Metric" /> representing the result of a gauge operation.
		/// </summary>
		/// <param name="name">Name or bucket for the metric</param>
		/// <param name="executionStatus">Result of the operation</param>
		/// <param name="workflowId">The ID which correlates the metric to a particular workflow.</param>
		/// <param name="value">GaugeOperation value</param>
		/// <param name="unitOfMeasure">Unit of measure describing a value (e.g. "document(s)")</param>
		/// <returns></returns>
		public static Metric GaugeOperation(string name, ExecutionStatus executionStatus, string workflowId, long value, string unitOfMeasure)
		{
			return new Metric(name, MetricType.GaugeOperation, workflowId)
			{
				ExecutionStatus = executionStatus,
				Value = value,
				CustomData = new Dictionary<string, object>() { { "unitOfMeasure", unitOfMeasure } }
			};
		}

		/// <summary>
		/// Create a <see cref="Metric"/> representing a point in time string value for system usage.
		/// </summary>
		/// <param name="name">The name of the metric.</param>
		/// <param name="value">The string value of the metric.</param>
		/// <param name="workflowId">The ID which correlates the metric to a particular workflow.</param>
		/// <returns>A <see cref="Metric"/> representation of the point in time string.</returns>
		public static Metric PointInTimeStringOperation(string name, string value, string workflowId)
		{
			return PointInTimeOperation(name, value, workflowId, MetricType.PointInTimeString);
		}

		/// <summary>
		/// Create a <see cref="Metric"/> representing a point in time long value for system usage.
		/// </summary>
		/// <param name="name">The name of the metric.</param>
		/// <param name="value">The long value of the metric.</param>
		/// <param name="workflowId">The ID which correlates the metric to a particular workflow.</param>
		/// <returns>A <see cref="Metric"/> representation of the point in time long.</returns>
		public static Metric PointInTimeLongOperation(string name, long value, string workflowId)
		{
			return PointInTimeOperation(name, value, workflowId, MetricType.PointInTimeLong);
		}

		/// <summary>
		/// Create a <see cref="Metric"/> representing a point in time double value for system usage.
		/// </summary>
		/// <param name="name">The name of the metric.</param>
		/// <param name="value">The double value of the metric.</param>
		/// <param name="workflowId">The ID which correlates the metric to a particular workflow.</param>
		/// <returns>A <see cref="Metric"/> representation of the point in time double.</returns>
		public static Metric PointInTimeDoubleOperation(string name, double value, string workflowId)
		{
			return PointInTimeOperation(name, value, workflowId, MetricType.PointInTimeDouble);
		}

		private static Metric PointInTimeOperation(string name, object value, string workflowId, MetricType type)
		{
			return new Metric(name, type, workflowId)
			{
				Value = value,
				WorkflowId = workflowId
			};
		}

		/// <summary>
		/// Creates a Dictionary out of the given <see cref="Metric" />'s public readable properties.
		/// </summary>
		public Dictionary<string, object> ToDictionary()
		{
			object GetValueAndConvertEnums(PropertyInfo p) => p.PropertyType.IsEnum ? p.GetValue(this).ToString() : p.GetValue(this);

			Dictionary<string, object> valueProperties = PublicInstanceValueProperties
				.ToDictionary(p => p.Name, GetValueAndConvertEnums);

			Dictionary<string, object> dictionaryProperties = PublicInstanceDictionaryProperties
				.Select(p => new { DictName = p.Name, Dict = (Dictionary<string, object>)p.GetValue(this) })
				.SelectMany(x => x.Dict.Select(kvp => new { Key = $"{x.DictName}.{kvp.Key}", kvp.Value }))
				.ToDictionary(x => x.Key, x => x.Value);

			Dictionary<string, object>[] dictionaries =
			{
				valueProperties,
				dictionaryProperties
			};
			Dictionary<string, object> dictionary = dictionaries
				.SelectMany(x => x)
				.ToDictionary(x => x.Key, x => x.Value);

			return dictionary;
		}

		/// <summary>
		/// Creates an array of <see cref="object" />s out of the the given <see cref="Metric" />'s public readable properties.
		/// </summary>
		public object[] ToPropertyArray()
		{
			return PublicInstanceProperties.Select(p => (object)new KeyValuePair<string, object>(p.Name, p.GetValue(this))).ToArray();
		}
	}
}