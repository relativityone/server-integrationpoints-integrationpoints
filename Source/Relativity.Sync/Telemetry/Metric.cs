using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Simple data bag representing a metric sent to a sink.
	/// </summary>
	internal class Metric
	{
		private IDictionary<string, object> _metadata;

		// Info for public properties with get methods on this class.
		// These are set by compile time, so we can calculate these ahead of time.
		private static readonly IEnumerable<PropertyInfo> _PUBLIC_READABLE_PROPERTIES = 
			typeof(Metric)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetMethod != null);

		private Metric(string name, MetricType type, string correlationId)
		{
			Name = name;
			Type = type;
			CorrelationId = correlationId;
		}

		/// <summary>
		///     Name of this metric.
		/// </summary>
		public string Name { get; }

		/// <summary>
		///     Type of metric this represents, e.g. a timed operation, a counter, etc.
		/// </summary>
		public MetricType Type { get; }

		/// <summary>
		///     ID that correlates logging and telemetry across a single job.
		/// </summary>
		public string CorrelationId { get; }

		/// <summary>
		///     Value of this metric.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		///     Status of the operation related to this metric.
		/// </summary>
		public ExecutionStatus ExecutionStatus { get; set; }

		/// <summary>
		///     Any metadata associated with this metric.
		/// </summary>
		public IDictionary<string, object> Metadata
		{
			get
			{
				if (_metadata == null)
				{
					_metadata = new Dictionary<string, object>();
				}
				return _metadata;
			}

			set
			{
				_metadata = value;
			}
		}

		/// <summary>
		///     Creates a <see cref="Metric"/> representing the result of a timed operation.
		/// </summary>
		/// <param name="name">Name or bucket for the metric</param>
		/// <param name="duration">Duration of the operation</param>
		/// <param name="executionStatus">Result of the oepration</param>
		/// <param name="correlationId">ID which correlates all metrics across a job</param>
		/// <returns></returns>
		public static Metric TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus, string correlationId)
		{
			return new Metric(name, MetricType.TimedOperation, correlationId)
			{
				Value = duration.TotalMilliseconds,
				ExecutionStatus = executionStatus
			};
		}

		/// <summary>
		/// Creates a Dictionary out of the given <see cref="Metric"/>'s public readable properties.
		/// </summary>
		public Dictionary<string, object> ToDictionary()
		{
			return _PUBLIC_READABLE_PROPERTIES.ToDictionary(p => p.Name, p => p.GetValue(this));
		}
	}
}
