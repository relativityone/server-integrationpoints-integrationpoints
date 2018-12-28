namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync job progress
	/// </summary>
	public sealed class SyncProgress
	{
		/// <summary>
		///     Sync job state
		/// </summary>
		public string State { get; }

		internal SyncProgress(string state)
		{
			State = state;
		}
	}
}