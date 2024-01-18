using System;

namespace Relativity.Sync.SyncConfiguration.FieldsMapping
{
    /// <summary>
    /// Occurs when invalid fields mapping is detected.
    /// </summary>
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

        /// <summary>
        /// Creates exception for a scenario when the identifier is mapped twice.
        /// </summary>
        public static InvalidFieldsMappingException IdentifierMappedTwice() =>
            new InvalidFieldsMappingException("Identifier cannot be mapped twice");

        /// <summary>
        /// Creates exception for a scenario when particular field name is not found.
        /// </summary>
        public static InvalidFieldsMappingException FieldNotFound(string fieldName) =>
            new InvalidFieldsMappingException($"Field not found - Name: {fieldName}");

        /// <summary>
        /// Creates exception for a scenario when particular field Artifact ID is not found.
        /// </summary>
        public static InvalidFieldsMappingException FieldNotFound(int fieldId) =>
            new InvalidFieldsMappingException($"Field not found - Id: {fieldId}");

        /// <summary>
        /// Creates exception for a scenario when field name is ambiguous.
        /// </summary>
        public static InvalidFieldsMappingException AmbiguousMatch(string fieldName) =>
            new InvalidFieldsMappingException($"Ambiguous match found - Name: {fieldName}");

        /// <summary>
        /// Creates exception for a scenario when the field is an identifier.
        /// </summary>
        public static InvalidFieldsMappingException FieldIsIdentifier(int fieldId) =>
            new InvalidFieldsMappingException($"Field is identifier - Id: {fieldId}");
    }
}
