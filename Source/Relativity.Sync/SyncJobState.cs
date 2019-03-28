namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync job progress
	/// </summary>
	public sealed class SyncJobState
	{
		/// <summary>
		///     Sync job state
		/// </summary>
		public string State { get; }

		internal SyncJobState(string state)
		{
			State = state;
		}
	}
}