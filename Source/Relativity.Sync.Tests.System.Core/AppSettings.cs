using System;
using System.Configuration;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace Relativity.Sync.Tests.System.Core
{
	public static class AppSettings
    {
        public static bool IsSettingsFileSet => TestContext.Parameters.Names.Any();

        public static string ServerBindingType => GetTestParameterStringValue("ServerBindingType");

        public static string RelativityHostName => GetTestParameterStringValue("RelativityHostAddress");

        public static Uri RelativityUrl => BuildHostNamedBasedUri("Relativity");

        public static Uri RelativityRestUrl => BuildHostNamedBasedUri("Relativity.Rest/api");

        public static Uri RelativityWebApiUrl => BuildHostNamedBasedUri("RelativityWebAPI");

        public static string RelativityUserName => GetTestParameterStringValue("AdminUsername");

        public static string RelativityUserPassword => GetTestParameterStringValue("AdminPassword");

        public static string SqlServer => GetTestParameterStringValue("SqlServer");

        public static string SqlUsername => GetTestParameterStringValue("SqlUsername");

        public static string SqlPassword => GetTestParameterStringValue("SqlPassword");

        public static string ConnectionStringEDDS => string.Format("Data Source={0};Initial Catalog=EDDS", SqlServer);

        public static string ConnectionStringWorkspace(int workspaceID) => string.Format("Data Source={0};Initial Catalog=EDDS{1}", SqlServer, workspaceID);

        public static bool SuppressCertificateCheck => GetTestParameterBooleanValue("SuppressCertificateCheck");

        public static string RelativeArchivesLocation => GetTestParameterStringValue("RelativeArchivesLocation");

        public static string RelativeBCPPathLocation => GetTestParameterStringValue("RelativeBCPPathLocation");

        public static string RemoteServerRoot => GetTestParameterStringValue("RemoteServerRoot");

        public static string RemoteArchivesLocation => GetTestParameterPathValue(RemoteServerRoot, RelativeArchivesLocation);

        public static string RemoteBCPPathLocation => GetTestParameterPathValue(RemoteServerRoot, RelativeBCPPathLocation);

        public static string ResourcePoolName => GetTestParameterStringValue("ResourcePoolName");

        public static string PerformanceResultsFilePath => GetTestParameterStringValue("PerformanceResultsFilePath");

        public static bool UseLogger => !GetTestParameterBooleanValue("SuppressCertificateCheck");

        public static int ArmRelativityTemplateMatterId => GetTestParameterIntegerValue("ArmRelativityTemplateMatterId");

        public static int ArmCacheLocationId => GetTestParameterIntegerValue("ArmCacheLocationId");

        public static int ArmFileRepositoryId => GetTestParameterIntegerValue("ArmFileRepositoryId");

        private static Uri BuildHostNamedBasedUri(string path)
        {
            string hostname = RelativityHostName;
            if (string.IsNullOrEmpty(hostname))
            {
                throw new ConfigurationErrorsException($"{nameof(hostname)} is not set. Please provide a value within the .runsettings file when running System tests.");
            }

            var uriBuilder = new UriBuilder(ServerBindingType, hostname, -1, path);
            return uriBuilder.Uri;
        }

        private static string GetTestParameterPathValue(string path1, string path2)
        {
            string value = Path.Combine(path1, path2);
            return value;
        }

        private static string GetTestParameterStringValue(string name)
        {
            string value = TestContext.Parameters[name];
            return value;
        }

        private static bool GetTestParameterBooleanValue(string name, bool defaultValue = false)
        {
            return bool.TryParse(GetTestParameterStringValue(name), out bool actualValue) ? actualValue : defaultValue;
        }

        private static int GetTestParameterIntegerValue(string name, int defaultValue = 0)
        {
            return int.TryParse(GetTestParameterStringValue(name), out int actualValue) ? actualValue : defaultValue;
        }
    }
}