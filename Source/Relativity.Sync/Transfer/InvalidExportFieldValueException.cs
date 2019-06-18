using System;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Exception thrown when the data returned from the Export API is not in a form recognized by Relativity Sync's internal sanitizers.
	/// This would likely be due to a change in contract with the Export API.
	/// </summary>
	[Serializable]
	public sealed class InvalidExportFieldValueException : Exception
	{
		/// <summary>
		/// Creates an instance of <see cref="InvalidExportFieldValueException"/>.
		/// </summary>
		/// <param name="sourceObjectName">Name of the object for which the error occurred</param>
		/// <param name="sourceFieldName">Name of the field on the object identified by <paramref name="sourceObjectName"/> for which the error occurred</param>
		/// <param name="message">Additional message describing the error</param>
		public InvalidExportFieldValueException(string sourceObjectName, string sourceFieldName, string message)
			: base($"{MessageTemplate(sourceObjectName, sourceFieldName)}: {message}")
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="InvalidExportFieldValueException"/>.
		/// </summary>
		/// <param name="sourceObjectName">Name of the object for which the error occurred</param>
		/// <param name="sourceFieldName">Name of the field on the object identified by <paramref name="sourceObjectName"/> for which the error occurred</param>
		/// <param name="message">Additional message describing the error</param>
		/// <param name="innerException">Exception which caused this exception to be thrown</param>
		public InvalidExportFieldValueException(string sourceObjectName, string sourceFieldName, string message, Exception innerException)
			: base($"{MessageTemplate(sourceObjectName, sourceFieldName)}: {message}", innerException)
		{
		}

		internal InvalidExportFieldValueException() : base()
		{
		}

		internal InvalidExportFieldValueException(string message) : base(message)
		{
		}

		internal InvalidExportFieldValueException(string message, Exception innerException) : base(message, innerException)
		{
		}

		private InvalidExportFieldValueException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
		}

		private static string MessageTemplate(string sourceObjectName, string sourceFieldName)
		{
			return $"Unable to parse data from Relativity Export API in field '{sourceFieldName}' of object '{sourceObjectName}'";
		}
	}
}
