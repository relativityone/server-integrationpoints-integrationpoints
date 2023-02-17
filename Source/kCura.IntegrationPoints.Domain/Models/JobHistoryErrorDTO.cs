namespace kCura.IntegrationPoints.Domain.Models
{
    /// <summary>
    /// DTO representation of Job History Error object
    /// </summary>
    public class JobHistoryErrorDTO : BaseDTO
    {
        public const string TableName = "JobHistoryError";

        /// <summary>
        /// Choices for Job History Error object
        /// </summary>
        public static class Choices
        {
            /// <summary>
            /// Choices for Error Type
            /// </summary>
            public static class ErrorType
            {
                public enum Values
                {
                    Item,
                    Job
                }
            }
        }

        /// <summary>
        /// Update Type used for which Statuses to Update to
        /// </summary>
        public class UpdateStatusType
        {
            /// <summary>
            /// Job Type
            /// </summary>
            public JobTypeChoices JobType { get; set; }

            /// <summary>
            /// Error Types for Retry
            /// </summary>
            public ErrorTypesChoices ErrorTypes { get; set; }

            public enum JobTypeChoices
            {
                RetryErrors,
                Run
            }

            public enum ErrorTypesChoices
            {
                JobOnly,
                JobAndItem,
                ItemOnly,
                None
            }

            public bool IsItemLevelErrorRetry()
            {
                return JobType == JobHistoryErrorDTO.UpdateStatusType.JobTypeChoices.RetryErrors &&
                       ErrorTypes == JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly;
            }
        }
    }
}
