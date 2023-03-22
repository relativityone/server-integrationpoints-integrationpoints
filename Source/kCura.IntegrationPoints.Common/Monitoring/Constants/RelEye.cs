namespace kCura.IntegrationPoints.Common.Monitoring.Constants
{
    public static class RelEye
    {
        /// <summary>
        /// RelEye Attributes - https://symmetrical-adventure-kvvzenn.pages.github.io/
        /// </summary>
        public static class Names
        {
            /// <summary>
            /// [string] Team identification.
            /// </summary>
            public const string R1TeamID = "r1.team.id";

            /// <summary>
            /// [string] Service name.
            /// </summary>
            public const string ServiceName = "service.name";

            /// <summary>
            /// [string] Toggle name.
            /// </summary>
            public const string ToggleName = "toggle.name";

            /// <summary>
            /// [string] Toggle value.
            /// </summary>
            public const string ToggleValue = "toggle.value";
        }

        public static class Values
        {
            /// <summary>
            /// Team identification in backstage.
            /// </summary>
            public const string R1TeamID = "PTCI-2456712";

            /// <summary>
            /// Service name.
            /// </summary>
            public const string ServiceName = "integrationpoints-relativity-sync";
        }

        public static class EventNames
        {
            public const string Toggle = "toggle_read";
        }
    }
}
