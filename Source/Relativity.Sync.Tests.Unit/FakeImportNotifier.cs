using System;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Tests.Unit
{
	internal sealed class FakeImportNotifier : IImportNotifier
	{
		public event IImportNotifier.OnCompleteEventHandler OnComplete;
		public event IImportNotifier.OnFatalExceptionEventHandler OnFatalException;
		public event IImportNotifier.OnProgressEventHandler OnProgress;
		public event IImportNotifier.OnProcessProgressEventHandler OnProcessProgress;

		public void RaiseOnProcessProgress(int failedItems, int totalItemsProcessed)
		{
			OnProcessProgress?.Invoke(new FullStatus(0, totalItemsProcessed, 0, failedItems, DateTime.MinValue, 
				DateTime.MinValue, string.Empty, string.Empty, 0, 0, Guid.Empty, null));
		}
	}
}