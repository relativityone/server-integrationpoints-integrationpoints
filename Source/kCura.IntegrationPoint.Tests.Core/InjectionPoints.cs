using kCura.Injection;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class InjectionPoints
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
	}
}