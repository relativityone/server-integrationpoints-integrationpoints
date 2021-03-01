using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Tests.Unit
{
	internal sealed class FakeImportNotifier : IImportNotifier
	{
		public event IImportNotifier.OnCompleteEventHandler OnComplete;
		public event IImportNotifier.OnProcessProgressEventHandler OnProcessProgress;

#pragma warning disable 67 // Event is never used
		public event IImportNotifier.OnFatalExceptionEventHandler OnFatalException;
		public event IImportNotifier.OnProgressEventHandler OnProgress;
#pragma warning restore 67 // Event is never used

		public void RaiseOnProcessProgress(int failedItems, int totalItemsProcessed)
		{
			OnProcessProgress?.Invoke(new FullStatus(0, totalItemsProcessed, 0, failedItems, DateTime.MinValue, 
				DateTime.MinValue, string.Empty, string.Empty, 0, 0, Guid.Empty, null));
		}

		public void RaiseOnProcessComplete(int failedItems, int totalItemsProcessed)
		{
			var jobReport = (JobReport)Activator.CreateInstance(typeof(JobReport), true);

			global::System.Reflection.FieldInfo errorRowsField = jobReport.GetType()
				.GetField("_errorRows", BindingFlags.Instance | BindingFlags.NonPublic);
			errorRowsField?.SetValue(jobReport, new JobReport.RowError[failedItems].ToList());

			PropertyInfo totalRows = jobReport.GetType()
				.GetProperty(nameof(jobReport.TotalRows));
			totalRows?.SetValue(jobReport, Convert.ChangeType(totalItemsProcessed, totalRows.PropertyType, CultureInfo.InvariantCulture),
				BindingFlags.NonPublic | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);

			OnComplete?.Invoke(jobReport);
		}
	}
}