namespace Relativity.Sync
{
	internal sealed class SyncProgress
	{
		public string State { get; }

		public SyncProgress(string state)
		{
			State = state;
		}
	}
}