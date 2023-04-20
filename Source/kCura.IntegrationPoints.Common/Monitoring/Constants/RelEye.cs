﻿namespace kCura.IntegrationPoints.Common.Monitoring.Constants
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
            /// [string] Service name.
            /// </summary>
            public const string ServiceVersion = "service.version";


            /// <summary>
            /// [string] Toggle name.
            /// </summary>
            public const string ToggleName = "toggle.name";

            /// <summary>
            /// [string] Toggle value.
            /// </summary>
            public const string ToggleValue = "toggle.value";

            /// <summary>
            /// [string] SourceId Name.
            /// </summary>
            public const string SourceId = "r1.source.id";
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
            public const string ToggleRead = "toggle_read";
        }

        public static class InstanceSettings
        {
            public const string REL_EYE_SECTION = "Relativity.Telemetry";
            public const string REL_EYE_TOKEN = "ReleyeToken";
            public const string REL_EYE_URI_LOGS = "ReleyeUriLogs";
            public const string CORE_SECTION = "Relativity.Core";
            public const string INSTANCE_IDENTIFIER = "InstanceIdentifier";
        }
    }
}