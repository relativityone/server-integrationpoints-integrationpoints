namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public class Monthly
    {
        public MonthlyType MonthChoice { get; set; }
        public int SelectedDay { get; set; }
        public OccuranceInMonth? SelectedType { get; set; }
        public DaysOfWeek SelectedDayOfTheMonth { get; set; }
        public string TemplateID { get; set; }

        public Monthly()
        {
            TemplateID = "monthlySendOn";
        }
    }
}