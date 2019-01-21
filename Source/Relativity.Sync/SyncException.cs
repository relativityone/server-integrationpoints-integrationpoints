using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync exception
	/// </summary>
	[Serializable]
	public sealed class SyncException : Exception
	{
		/// <summary>
		///     Sync job correlation ID
		/// </summary>
		public string CorrelationId { get; }

		/// <inheritdoc />
		public SyncException()
		{
		}

		/// <inheritdoc />
		private SyncException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			CorrelationId = info.GetString(nameof(CorrelationId));
		}

		/// <inheritdoc />
		public SyncException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public SyncException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		///     Constructor with correlation ID
		/// </summary>
		public SyncException(string message, string correlationId) : base(message)
		{
			CorrelationId = correlationId;
		}

		/// <summary>
		///     Constructor with correlation ID
		/// </summary>
		public SyncException(string message, Exception innerException, string correlationId) : base(message, innerException)
		{
			CorrelationId = correlationId;
		}

		/// <summary>
		/// </summary>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			info.AddValue(nameof(CorrelationId), CorrelationId);
			base.GetObjectData(info, context);
		}
	}
}