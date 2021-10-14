namespace Relativity.IntegrationPoints.Tests.Functional
{
	internal static class Const
	{
		public const string INTEGRATION_POINTS_AGENT_TYPE_NAME = "Integration Points Agent";
		public const int INTEGRATION_POINTS_AGENT_RUN_INTERVAL = 5;

		public static class Application
		{
			public const string INTEGRATION_POINTS_APPLICATION_NAME = "Integration Points";
			public const string LEGAL_HOLD_APPLICATION_NAME = "Relativity Legal Hold";
			public const string ART_TEST_SERVICES_APPLICATION_NAME = "ARM Test Services";
		}

		public static class XSS
		{
			public const string XSS_JS = "';window.relativityXss=true;";
		}
	}
}
