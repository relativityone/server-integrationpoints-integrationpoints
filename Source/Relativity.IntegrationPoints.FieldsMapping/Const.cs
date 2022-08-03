namespace Relativity.IntegrationPoints.FieldsMapping
{
    public static class FieldTypeName
    {
        public const string DATE = "Date";
        public const string DECIMAL = "Decimal";
        public const string FIXED_LENGTH_TEXT = "Fixed-Length Text";
        public const string LONG_TEXT = "Long Text";
        public const string MULTIPLE_CHOICE = "Multiple Choice";
        public const string MULTIPLE_OBJECT = "Multiple Object";
        public const string SINGLE_CHOICE = "Single Choice";
        public const string SINGLE_OBJECT = "Single Object";
        public const string YES_NO = "Yes/No";
    }

    public static class InvalidMappingReasons
    {
        public const string _INCOMPATIBLE_TYPES = "Types are not compatible";
        public const string _UNICODE_DIFFERENCE = "Unicode is different";
        public const string _UNSUPORTED_TYPES = "Selected fields types might fail the job";
        public const string _FIELD_IDENTIFIERS_NOT_MAPPED = "Field identifiers should be mapped with each other";
        public const string _FIELD_IS_MISSING = "One of fields is missing";
    }
}
