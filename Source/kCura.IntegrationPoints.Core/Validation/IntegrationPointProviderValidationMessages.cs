using kCura.IntegrationPoints.Core.Contracts.Entity;

namespace kCura.IntegrationPoints.Core.Validation
{
    public static class IntegrationPointProviderValidationMessages
    {
        public static readonly string WORKSPACE_NOT_EXIST = "workspace does not exist.";

        public static readonly string ARTIFACT_NOT_EXIST = "artifact does not exists.";

        public static readonly string NEXT_BUTTON_INSTRUCTION = " Click \"Next\" and fill in the missing value(s).";

        public static readonly string ERROR_INTEGRATION_POINT_NAME_EMPTY = "Integration Point name cannot be empty.";

        public static readonly string ERROR_INTEGRATION_POINT_NAME_CONTAINS_ILLEGAL_CHARACTERS = "Integration Point name contains illegal characters (<>:\\\"\\\\\\/|\\?\\* TAB).";

        public static readonly string ERROR_DESTINATON_LOCATION_EMPTY = "Destination location cannot be empty.";

        public static readonly string ERROR_INVALID_EMAIL = "E-mail format is invalid: ";

        public static readonly string ERROR_MISSING_EMAIL = "Missing email.";

        public static readonly string ERROR_EMAIL_VALIDATION_EXCEPTION = "Email Validation exception: ";

        public static readonly string ERROR_SCHEDULER_NOT_INITIALIZED = "Scheduler object not initialized";

        public static readonly string ERROR_SCHEDULER_REQUIRED_VALUE = "This field is required: ";

        public static readonly string ERROR_SCHEDULER_INVALID_DATE_FORMAT = "Invalid string representation of a date: ";

        public static readonly string ERROR_SCHEDULER_INVALID_TIME_FORMAT = "Invalid string representation of a time: ";

        public static readonly string ERROR_SCHEDULER_NOT_IN_RANGE = " value not in range: ";

        public static readonly string ERROR_SCHEDULER_INVALID_VALUE = "Invalid value for: ";

        public static readonly string ERROR_SCHEDULER_END_DATE_BEFORE_START_DATE = "The start date must come before the end date.";

        public static readonly string ERROR_INTEGRATION_POINT_TYPE_INVALID = "Invalid integration point type for given source provider.";

        public static readonly string ERROR_MISSING_FIRST_NAME_FIELD_MAP = $"Field: \"{EntityFieldNames.FirstName}\" should be mapped in Destination";

        public static readonly string ERROR_MISSING_LAST_NAME_FIELD_MAP = $"Field: \"{EntityFieldNames.LastName}\" should be mapped in Destination";
    }
}
