using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
    public static class TextWriterExtensions
    {
        public static void Log(this TextWriter writer, string message)
        {
            string logMessage = $"[{DateTime.Now.ToLongTimeString()}] {message}";
            writer.WriteLine(logMessage);
        }

        public static void Log(this TextWriter writer, string message, Exception ex)
        {
            writer.Log(message + Environment.NewLine + ex.ToString());
        }

        public static void Log(this TextWriter writer, IDictionary<string, object> stuff)
        {
            string message = string.Empty;
            stuff.ToList().ForEach(pair => message += $"{pair.Key}: {pair.Value?.ToString() ?? "NULL"}{Environment.NewLine}");
            writer.Log(message);
        }
    }
}
