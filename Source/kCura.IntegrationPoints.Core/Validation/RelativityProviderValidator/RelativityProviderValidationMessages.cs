namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
    public static class RelativityProviderValidationMessages
    {
        public static readonly string FIELD_MAP_DESTINATION_FIELD_NOT_MAPPED = "All selected fields must be mapped. Destination field not mapped to Source: ";

        public static readonly string FIELD_MAP_FIELD_IS_IDENTIFIER = "Is Identifier";

        public static readonly string FIELD_MAP_FIELD_MUST_BE_MAPPED = "must be mapped.";

        public static readonly string FIELD_MAP_FIELD_NAME = "Name";

        public static readonly string FIELD_MAP_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE = "Field does not exist in source workspace.";

        public static readonly string FIELD_MAP_IDENTIFIERS_NOT_MATCHED = "Identifier must be mapped with another identifier.";

        public static readonly string FIELD_MAP_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED = "All selected fields must be mapped. Destination and Source fields not mapped.";

        public static readonly string FIELD_MAP_SOURCE_FIELD_NOT_MAPPED = "All selected fields must be mapped. Source field not mapped to Destination: ";

        public static readonly string FIELD_MAP_UNIQUE_IDENTIFIER_MUST_BE_MAPPED = "The unique identifier must be mapped.";

        public static readonly string FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_INVALID = "Invalid Overlay Behavior: ";

        public static readonly string FIELD_MAP_APPEND_ONLY_INVALID_OVERLAY_BEHAVIOR = "For Append Only should be set \"Use Field Settings\" overlay behaior.";

        public static readonly string FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_MERGE = "Merge Values";

        public static readonly string FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_REPLACE = "Replace Values";

        public static readonly string FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT = "Use Field Settings";

        public static readonly string FIELD_MAP_DYNAMIC_FOLDER_PATH_AND_FOLDER_PATH_INFORMATION_CONFLICT = "You cannot use Folder Path Information from Document field when using Dynamic Folder Path generation.";

        public static readonly string FIELD_MAP_IMAGE_TOO_MANY_FIELDS = "Too many fields mapped. Please map only identifier fields.";

        public static readonly string WORKSPACE_INVALID_NAME = "workspace name contains an invalid character.";
    }
}
