using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public delegate void JobError(Exception ex);
	public delegate void RowError(string documentIdentifier, string errorMessage);
	public delegate void BatchCompleted(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount);
	public delegate void BatchSubmitted(int size, int batchSize);
	public delegate void BatchCreated(int batchSize);

	public interface IImportService
	{
		void AddRow(Dictionary<string, object> fields);
		bool PushBatchIfFull(bool forcePush);
		void Initialize();
		void CleanUp();
		void KickOffImport(IDataReader dataSource);
		ImportSettings Settings { get; }

		event BatchCompleted OnBatchComplete;
		event BatchSubmitted OnBatchSubmit;
		event BatchCreated OnBatchCreate;
		event JobError OnJobError;
		event RowError OnDocumentError;
	}
}
