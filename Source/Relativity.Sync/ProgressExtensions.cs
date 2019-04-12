﻿using System;

namespace Relativity.Sync
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
			SyncJobState jobState = SyncJobState.Completed(id);
			progress.Report(jobState);
		}

		/// <summary>
		///     Reports that a given Sync step has completed with errors.
		/// </summary>
		/// <param name="progress">Progress reporter</param>
		/// <param name="id">ID of the Sync step</param>
		public static void ReportCompletedWithErrors(this IProgress<SyncJobState> progress, string id)
		{
			SyncJobState jobState = SyncJobState.CompletedWithErrors(id);
			progress.Report(jobState);
		}

		/// <summary>
		///     Creates an aggregated <see cref="IProgress{SyncJobState}"/> out of an array of <see cref="IProgress{SyncJobState}"/>s.
		/// </summary>
		/// <param name="progressReporters">Collection of <see cref="IProgress{SyncJobState}"/> to combine. Must be non-null.</param>
		/// <returns>Single <see cref="IProgress{SyncJobState}"/> that will invoke all underlying implementations on <see cref="IProgress{T}.Report"/>.</returns>
		public static IProgress<SyncJobState> Combine(this IProgress<SyncJobState>[] progressReporters)
		{
			if (progressReporters == null)
			{
				throw new ArgumentNullException(nameof(progressReporters));
			}

			IProgress<SyncJobState> aggregatedProgress;

			if (progressReporters.Length == 0)
			{
				aggregatedProgress = new EmptyProgress<SyncJobState>();
			}
			else if (progressReporters.Length == 1)
			{
				aggregatedProgress = progressReporters[0];
			}
			else
			{
				aggregatedProgress = new AggregateProgress<SyncJobState>(progressReporters);
			}

			return aggregatedProgress;
		}
	}
}