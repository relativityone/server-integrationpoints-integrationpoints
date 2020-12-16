using System;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.FieldsMapping
{
	/// <inheritdoc />
	[Serializable]
	public sealed class InvalidFieldsMappingException : Exception
	{
		/// <inheritdoc />
		public InvalidFieldsMappingException()
		{
		}

		/// <inheritdoc />
		public InvalidFieldsMappingException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public InvalidFieldsMappingException(string message, Exception inner) : base(message, inner)
		{
		}

		public static InvalidFieldsMappingException IdentifierMappedTwice() =>
			new InvalidFieldsMappingException("Identifier cannot be mapped twice");

		public static InvalidFieldsMappingException FieldNotFound(string fieldName) =>
			new InvalidFieldsMappingException($"Field not found - Name: {fieldName}");

		public static InvalidFieldsMappingException FieldNotFound(int fieldId) =>
			new InvalidFieldsMappingException($"Field not found - Id: {fieldId}");

		public static InvalidFieldsMappingException AmbiguousMatch(string fieldName) =>
			new InvalidFieldsMappingException($"Ambiguous match found - Name: {fieldName}");

		public static InvalidFieldsMappingException FieldIsIdentifier(int fieldId) =>
			new InvalidFieldsMappingException($"Field is identifier - Id: {fieldId}");
	}
}
