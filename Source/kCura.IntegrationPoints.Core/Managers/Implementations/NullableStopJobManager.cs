using System;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class NullableStopJobManager : IJobStopManager
	{
		public object SyncRoot { get; }

		public NullableStopJobManager()
		{
			SyncRoot = new object();
		}

		public void Dispose()
		{
			// Nullable pattern. This method does not do anything
		}

		public bool IsStopRequested()
		{
			return false;
		}

		public void ThrowIfStopRequested()
		{
			// Nullable pattern. This method does not do anything
		}

		public event EventHandler<EventArgs> StopRequestedEvent;

		protected virtual void RaiseStopRequestedEvent()
		{
			StopRequestedEvent?.Invoke(this, EventArgs.Empty);
		}
	}
}