using System.Collections.Generic;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public class Weekly
    {
        public List<string> SelectedDays { get; set; }
        public string TemplateID { get; set; }

        public Weekly()
        {
            TemplateID = "weeklySendOn";
        }
    }
}