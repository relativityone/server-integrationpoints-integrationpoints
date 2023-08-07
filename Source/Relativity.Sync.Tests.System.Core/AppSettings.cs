using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Relativity.Sync.Tests.System.Core
{
    public static class AppSettings
    {
		public static bool IsSettingsFileSet => TestContext.Parameters.Names.Any();

		public static string ServerBindingType => TestContext.Parameters["ServerBindingType"];

		public static string RelativityHostName => TestContext.Parameters["RelativityHostAddress"];

		public static Uri RelativityUrl => BuildRelativityUri();

        public static Uri RelativityRestUrl => BuildRelativityRestUri();

        public static Uri RelativityWebApiUrl => BuildRelativityWebApiUri();

        public static string RelativityUserName => TestContext.Parameters["AdminUsername"];

        public static string RelativityUserPassword => TestContext.Parameters["AdminPassword"];
        
        public static string SqlServer => TestContext.Parameters["SqlServer"];

		public static string SqlUsername => TestContext.Parameters["SqlUsername"];

		public static string SqlPassword => TestContext.Parameters["SqlPassword"];

		public static string ConnectionStringEDDS => string.Format("Data Source={0};Initial Catalog=EDDS", SqlServer);

        public static string ConnectionStringWorkspace(int workspaceID) => string.Format("Data Source={0};Initial Catalog=EDDS{1}", SqlServer, workspaceID);

        public static bool SuppressCertificateCheck => bool.Parse(TestContext.Parameters["SuppressCertificateCheck"]);

        public static string RelativeArchivesLocation => GetConfigValue("RelativeArchivesLocation");

        public static string RelativeBCPPathLocation => GetConfigValue("RelativeBCPPathLocation");

        public static string RemoteServerRoot => GetConfigValue("RemoteServerRoot");

        public static string RemoteArchivesLocation => Path.Combine(RemoteServerRoot, RelativeArchivesLocation);

        public static string RemoteBCPPathLocation => Path.Combine(RemoteServerRoot, RelativeBCPPathLocation);

        public static string ResourcePoolName => GetConfigValue("ResourcePoolName");

        public static string PerformanceResultsFilePath => GetConfigValue("PerformanceResultsFilePath");

        public static bool UseLogger => !bool.TryParse(GetConfigValue("SuppressCertificateCheck"), out bool useLogger) || useLogger;

        public static int ArmRelativityTemplateMatterId => int.Parse(GetConfigValue("ArmRelativityTemplateMatterId"));

        public static int ArmCacheLocationId => int.Parse(GetConfigValue("ArmCacheLocationId"));

        public static int ArmFileRepositoryId => int.Parse(GetConfigValue("ArmFileRepositoryId"));

        public static string DataTransferLegacyPath => Path.Combine(GetConfigValue("BuildToolsDirectory"), GetConfigValue("DataTransferLegacyPath"));

        private static string GetConfigValue(string name) => TestContext.Parameters.Exists(name)
            ? TestContext.Parameters[name]
            : ConfigurationManager.AppSettings.Get(name);

        private static Uri BuildRelativityUri()
        {
	        var uriBuilder = new UriBuilder(ServerBindingType, RelativityHostName, -1, "/Relativity");
	        return uriBuilder.Uri;
        }

        private static Uri BuildRelativityRestUri()
        {
	        var uriBuilder = new UriBuilder(ServerBindingType, RelativityHostName, -1, "/Relativity.Rest/api");
	        return uriBuilder.Uri;
        }

        private static Uri BuildRelativityWebApiUri()
        {
	        var uriBuilder = new UriBuilder(ServerBindingType, RelativityHostName, -1, "/RelativityWebAPI");
	        return uriBuilder.Uri;
		}
	}
}
