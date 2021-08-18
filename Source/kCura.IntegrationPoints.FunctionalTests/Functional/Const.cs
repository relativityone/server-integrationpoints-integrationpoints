namespace Relativity.IntegrationPoints.Tests.Functional
{
	internal static class Const
	{
		public const string INTEGRATION_POINTS_APPLICATION_NAME = "Integration Points";
		public const string INTEGRATION_POINTS_APPLICATION_GUID = "dcf6e9d1-22b6-4da3-98f6-41381e93c30c";

		public const string INTEGRATION_POINTS_AGENT_TYPE_NAME = "Integration Points Agent";
		public const int INTEGRATION_POINTS_AGENT_RUN_INTERVAL = 5;

		public static class XSS
		{
			public const string XSS_JS = "';window.relativityXss=true;";
		}
	}
}
