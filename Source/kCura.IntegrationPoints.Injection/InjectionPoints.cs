using kCura.Injection;

namespace kCura.IntegrationPoints.Injection
{
	public static class InjectionPoints
	{
		public const string INTEGRATION_POINTS_FEATURE = "Integration Points";

		public static readonly InjectionPoint BEFORE_JOB_HISTORY_ERRORS_STATUS_UPDATE =
			new InjectionPoint("A876A7F9-A9F8-445C-9A01-FCB0C7FD4E8B",
				"After the agent picks up a job but before temp tables for JobHistoryErrors are deleted",
				INTEGRATION_POINTS_FEATURE);

		public static readonly InjectionPoint BEFORE_JOB_HISTORY_ERRORS_TEMP_TABLE_REMOVAL =
			new InjectionPoint("C2B46E70-20EF-4A08-8BCF-9A15274ECC55",
				"After the agent picks up a job but before JobHistoryErrors are updated",
				INTEGRATION_POINTS_FEATURE);

		public static readonly InjectionPoint BEFORE_AGENT_CREATES_TASK =
			new InjectionPoint("624EF1A2-CFE5-4C99-A72B-89A65BB02CC5",
				"Before agent checks for synchronization and decides which type of task to create",
				INTEGRATION_POINTS_FEATURE);

		public static readonly InjectionPoint BEFORE_TAGGING_STARTS_ONJOBCOMPLETE =
			new InjectionPoint("86D2AD3B-7C77-4CC9-8765-5FAAEF7A12E3",
				"Before tagging starts in TargetDocumentsTaggingManager.OnJobComplete",
				INTEGRATION_POINTS_FEATURE);
	}
}