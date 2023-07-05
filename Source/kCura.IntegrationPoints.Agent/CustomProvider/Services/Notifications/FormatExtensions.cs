using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications
{
    internal static class FormatExtensions
    {
        public static string ToBoldedHtmlFont(this string input)
        {
            return $"<b>{input}</b>";
        }

        public static string ToH2HeaderHtml(this string input)
        {
            return $"<h2>{input}</h2>";
        }
    }
}
