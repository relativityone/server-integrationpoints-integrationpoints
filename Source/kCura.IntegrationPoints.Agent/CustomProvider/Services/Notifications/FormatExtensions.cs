using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications
{
    internal static class FormatExtensions
    {
        public static string ToLineWithBoldedSectionHtml(this string input, string injectedValue)
        {
            return $"<b>{input}</b>{injectedValue}<br>";
        }

        public static string ToH3HeaderHtml(this string input)
        {
            return $"<h3>{input}</h3>";
        }
    }
}
