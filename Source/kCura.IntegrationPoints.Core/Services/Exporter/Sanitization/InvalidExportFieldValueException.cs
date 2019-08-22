using System;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	/// <summary>
	/// Exception thrown when the data returned from the Export API is not in a form recognized by RIP old sync flow internal sanitizers.
	/// This would likely be due to a change in contract with the Export API.
	/// </summary>
	[Serializable]
	public sealed class InvalidExportFieldValueException : IntegrationPointsException
	{
		/// <summary>
		/// Creates an instance of <see cref="InvalidExportFieldValueException"/>.
		/// </summary>
		/// <param name="sourceObjectName">Name of the object for which the error occurred.</param>
		/// <param name="sourceFieldName">Name of the field on the object identified by <paramref name="sourceObjectName"/> for which the error occurred.</param>
		/// <param name="message">Additional message describing the error.</param>
		internal InvalidExportFieldValueException(string sourceObjectName, string sourceFieldName, string message)
			: base(GetMessage(sourceObjectName, sourceFieldName, message))
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="InvalidExportFieldValueException"/>.
		/// </summary>
		/// <param name="sourceObjectName">Name of the object for which the error occurred.</param>
		/// <param name="sourceFieldName">Name of the field on the object identified by <paramref name="sourceObjectName"/> for which the error occurred.</param>
		/// <param name="message">Additional message describing the error.</param>
		/// <param name="innerException">Original thrown exception.</param>
		internal InvalidExportFieldValueException(string sourceObjectName, string sourceFieldName, string message, Exception innerException)
			: base(GetMessage(sourceObjectName, sourceFieldName, message), innerException)
		{
		}

		/// <inheritdoc />
		public InvalidExportFieldValueException() { }

		/// <inheritdoc />
		public InvalidExportFieldValueException(string message) : base(message) { }

		/// <inheritdoc />
		public InvalidExportFieldValueException(string message, Exception innerException) : base(message, innerException) { }

		private static string GetMessage(string sourceObjectName, string sourceFieldName, string message)
		{
			return $"Unable to parse data from Relativity Export API in field '{sourceFieldName}' of object '{sourceObjectName}'. {message}";
		}
	}
}
