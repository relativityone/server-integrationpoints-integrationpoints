using System;
using kCura.Relativity.DataReaderClient;
using kCura.Utility.Extensions;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed class FakeImportNotifier : IImportNotifier
	{
#pragma warning disable 67
		public event IImportNotifier.OnCompleteEventHandler OnComplete;
		public event IImportNotifier.OnFatalExceptionEventHandler OnFatalException;
		public event IImportNotifier.OnProgressEventHandler OnProgress;
#pragma warning restore 67
		public event IImportNotifier.OnProcessProgressEventHandler OnProcessProgress;

		public void RaiseOnProcessProgress(int failedItems, int totalItemsProcessed)
		{
			OnProcessProgress?.Invoke(new FullStatus(0, totalItemsProcessed, 0, failedItems, DateTime.MinValue, 
				DateTime.MinValue, string.Empty, string.Empty, 0, 0, Guid.Empty, null));
		}

		public void RaiseOnProcessComplete()
		{
			var jobReport = (JobReport)Activator.CreateInstance(typeof(JobReport), true);
			OnComplete?.Invoke(jobReport);
		}
	}
}