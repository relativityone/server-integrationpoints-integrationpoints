using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications
{
    internal static class HtmlFormatExtensions
    {
        public static string ToLineWithBoldedSectionHtml(this string input, string injectedValue)
        {
            return $"<b>{input}</b>{injectedValue}<br>";
        }

        public static string ToH3HeaderHtml(this string input)
        {
            return $"<h3>{input}</h3>";
        }

        public static string ToJobStatisticsFormattedHtml(this string input, Data.JobHistory jobHistory)
        {
            return $@"<b>{input}</b><br>
                      &emsp; Total Items: {jobHistory.TotalItems ?? 0}<br>
                      &emsp; Transferred Items: {jobHistory.ItemsTransferred ?? 0}<br>
                      &emsp; Items with Errors: {jobHistory.ItemsWithErrors ?? 0}<br>";
        }
    }
}
