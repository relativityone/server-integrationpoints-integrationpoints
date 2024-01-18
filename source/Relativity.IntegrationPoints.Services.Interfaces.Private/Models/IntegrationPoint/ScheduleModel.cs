namespace Relativity.IntegrationPoints.Services
{
    public class ScheduleModel
    {
        public bool EnableScheduler { get; set; }

        public string EndDate { get; set; }

        public int TimeZoneOffsetInMinute { get; set; }

        public string StartDate { get; set; }

        public string SelectedFrequency { get; set; }

        public int Reoccur { get; set; }

        public string ScheduledTime { get; set; }

        public string SendOn { get; set; }

        public string TimeZoneId { get; set; }
    }
}
