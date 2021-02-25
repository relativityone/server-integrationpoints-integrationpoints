﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync job
	/// </summary>
	public interface ISyncJob
	{
		/// <summary>
		///     Executes job
		/// </summary>
		/// <param name="token">Cancellation token</param>
		Task ExecuteAsync(CancellationToken token);

		/// <summary>
		///     Executes job
		/// </summary>
		/// <param name="progress">The progress object</param>
		/// <param name="token">Cancellation token</param>
		Task ExecuteAsync(IProgress<SyncJobState> progress, CancellationToken token);
	}
}