namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents the schedule configuration for an integration point.
    /// </summary>
    public class ScheduleModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the scheduler is enabled.
        /// </summary>
        public bool EnableScheduler { get; set; }

        /// <summary>
        /// Gets or sets the end date of the schedule.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the time zone offset in minutes.
        /// </summary>
        public int TimeZoneOffsetInMinute { get; set; }

        /// <summary>
        /// Gets or sets the start date of the schedule.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Gets or sets the selected frequency of the schedule.
        /// </summary>
        public string SelectedFrequency { get; set; }

        /// <summary>
        /// Gets or sets the reoccur interval of the schedule.
        /// </summary>
        public int Reoccur { get; set; }

        /// <summary>
        /// Gets or sets the scheduled time of the day.
        /// </summary>
        public string ScheduledTime { get; set; }

        /// <summary>
        /// Gets or sets the day on which the schedule is to be sent.
        /// </summary>
        public string SendOn { get; set; }

        /// <summary>
        /// Gets or sets the time zone ID for the schedule.
        /// </summary>
        public string TimeZoneId { get; set; }
    }
}
