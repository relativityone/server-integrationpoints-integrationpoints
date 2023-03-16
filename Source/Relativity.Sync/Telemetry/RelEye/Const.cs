namespace Relativity.Sync.Telemetry.RelEye
{
    internal static class Const
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
            /// [string] Job CorrelationID for Sync Job
            /// </summary>
            public const string WorkflowId = "job.workflow_id";

            /// <summary>
            /// [string] Job end status.
            /// </summary>
            public const string JobResult = "job.result";

            /// <summary>
            /// [string] Job type (Document, Entity, etc.).
            /// </summary>
            public const string JobType = "job.type";
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
            public const string ServiceName = "relativity-sync-rap";
        }
    }
}
