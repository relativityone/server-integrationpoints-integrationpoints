namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class SchedulerModel
    {
        public const string FrequencySelect = "Select...";
        public const string FrequencyDaily = "Daily";
        public const string FrequencyWeekly = "Weekly";
        public const string FrequencyMonthly = "Monthly";
        public const string TimeMeridiemAM = "AM";
        public const string TimeMeridiemPM = "PM";

        public SchedulerModel(bool enable)
        {
            Enable = enable;
        }

        public bool Enable { get; set; }

        public string Frequency { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string Time { get; set; }

        public string TimeMeridiem { get; set; }
    }
}
