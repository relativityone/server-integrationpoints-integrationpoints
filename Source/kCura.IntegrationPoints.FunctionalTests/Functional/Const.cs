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
            public const string ARM_TEST_SERVICES_APPLICATION_NAME = "ARM Test Services";
            public const string DATA_TRANSFER_LEGACY = "DataTransfer.Legacy";
        }

        public static class XSS
        {
            public const string XSS_JS = "';window.relativityXss=true;";
        }

        public static class ImportLoadFile
        {
            public const int ASCII_COLUMN = 20;
            public const int ASCII_QUOTE = 254;
            public const int ASCII_NEWLINE = 174;
            public const int ASCII_MULTILINE = 59;
            public const int ASCII_NESTEDVALUE = 92;
        }
    }
}
