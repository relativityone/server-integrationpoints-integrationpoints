using System;
using kCura.WinEDDS.Api;
using Relativity.DataExchange.Io;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.LoadFile
{
	public class FakeLoadFileArtifactReader : IArtifactReader
	{
		public ArtifactFieldCollection ReadArtifact()
		{
			throw new NotImplementedException();
		}

		public string[] GetColumnNames(object args)
		{
			return new[] {"Control Number"};
		}

		public void ValidateColumnNames(Action<string> invalidNameAction)
		{
			throw new NotImplementedException();
		}

		public long? CountRecords()
		{
			throw new NotImplementedException();
		}

		public string SourceIdentifierValue()
		{
			throw new NotImplementedException();
		}

		public void AdvanceRecord()
		{
			throw new NotImplementedException();
		}

		public void Close()
		{
			throw new NotImplementedException();
		}

		public string ManageErrorRecords(string errorMessageFileLocation, string prePushErrorLineNumbersFileName)
		{
			throw new NotImplementedException();
		}

		public void OnFatalErrorState()
		{
			throw new NotImplementedException();
		}

		public void Halt()
		{
			throw new NotImplementedException();
		}

		public bool HasMoreRecords { get; }
		public int CurrentLineNumber { get; }
		public long SizeInBytes { get; }
		public long BytesProcessed { get; }

		public event IArtifactReader.OnIoWarningEventHandler OnIoWarning;
		public event IArtifactReader.DataSourcePrepEventHandler DataSourcePrep;
		public event IArtifactReader.StatusMessageEventHandler StatusMessage;
		public event IArtifactReader.FieldMappedEventHandler FieldMapped;

		protected virtual void OnOnIoWarning(IoWarningEventArgs e)
		{
			OnIoWarning?.Invoke(e);
		}

		protected virtual void OnDataSourcePrep(DataSourcePrepEventArgs e)
		{
			DataSourcePrep?.Invoke(e);
		}

		protected virtual void OnStatusMessage(string message)
		{
			StatusMessage?.Invoke(message);
		}

		protected virtual void OnFieldMapped(string sourcefield, string workspacefield)
		{
			FieldMapped?.Invoke(sourcefield, workspacefield);
		}
	}
}