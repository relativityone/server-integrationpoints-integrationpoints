using System;

namespace Relativity.Sync.Nodes
{
	internal static class ProgressExtensions
	{
		/// <summary>
		///     Reports that a given Sync step has been started.
		/// </summary>
		/// <param name="progress">Progress reporter</param>
		/// <param name="id">ID of the Sync step</param>
		public static void ReportStarted(this IProgress<SyncJobState> progress, string id)
		{
			SyncJobState jobState = SyncJobState.Start(id);
			progress.Report(jobState);
		}

		/// <summary>
		///     Reports that a given Sync step has failed.
		/// </summary>
		/// <param name="progress">Progress reporter</param>
		/// <param name="id">ID of the Sync step</param>
		/// <param name="exception">Exception that caused the failure</param>
		public static void ReportFailure(this IProgress<SyncJobState> progress, string id, Exception exception)
		{
			SyncJobState jobState = SyncJobState.Failure(id, exception);
			progress.Report(jobState);
		}

		/// <summary>
		///     Reports that a given Sync step has been canceled.
		/// </summary>
		/// <param name="progress">Progress reporter</param>
		/// <param name="id">ID of the Sync step</param>
		public static void ReportCanceled(this IProgress<SyncJobState> progress, string id)
		{
			SyncJobState jobState = SyncJobState.Canceled(id);
			progress.Report(jobState);
		}

		/// <summary>
		///     Reports that a given Sync step has completed.
		/// </summary>
		/// <param name="progress">Progress reporter</param>
		/// <param name="id">ID of the Sync step</param>
		public static void ReportCompleted(this IProgress<SyncJobState> progress, string id)
		{
			SyncJobState jobState = SyncJobState.CompletedWithErrors(id);
			progress.Report(jobState);
		}

		/// <summary>
		///     Reports that a given Sync step has completed with errors.
		/// </summary>
		/// <param name="progress">Progress reporter</param>
		/// <param name="id">ID of the Sync step</param>
		public static void ReportCompletedWithErrors(this IProgress<SyncJobState> progress, string id)
		{
			SyncJobState jobState = SyncJobState.Completed(id);
			progress.Report(jobState);
		}
	}
}