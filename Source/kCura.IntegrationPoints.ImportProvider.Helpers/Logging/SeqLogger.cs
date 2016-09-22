using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace kCura.IntegrationPoints.ImportProvider.Helpers.Logging
{
    public class SeqLogger
    {
        public static void Info(string template, params object[] args)
        {
            CreateLogger();
            Log.Information(template, args);
            DestroyLogger();
        }

        public static void Warn(string template, params object[] args)
        {
            CreateLogger();
            Log.Warning(template, args);
            DestroyLogger();
        }

        public static void Error(string template, params object[] args)
        {
            CreateLogger();
            Log.Error(template, args);
            DestroyLogger();
        }

        private static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
        }

        private static void DestroyLogger()
        {
            Log.CloseAndFlush();
        }
    }
}
