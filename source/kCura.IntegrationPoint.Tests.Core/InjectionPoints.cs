namespace kCura.IntegrationPoint.Tests.Core
{
	public class InjectionPoints
	{
		/// <summary>
		/// After the agent picks up a job but before temp tables for JobHistoryErrors are deleted .
		/// </summary>
		public const string BeforeJobHistoryErrorsStatusUpdate = "A876A7F9-A9F8-445C-9A01-FCB0C7FD4E8B";

		/// <summary>
		/// After the agent picks up a job but before JobHistoryErrors are updated.
		/// </summary>
		public const string BeforeJobHistoryErrorsTempTableRemoval = "C2B46E70-20EF-4A08-8BCF-9A15274ECC55";
	}
}
