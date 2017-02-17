using System;
using System.Data;
using kCura.IntegrationPoints.Domain.Readers;

namespace kCura.IntegrationPoints.Core.Services.Exporter.TransferContext
{
	public class ImageTransferContext:IDataTransferContext
	{
		public ImageTransferContext(IDataReader  reader)
		{
			DataReader = reader;
		}

		public bool HasDynamicRecordsCount { get; }
		public IDataReader DataReader { get; set; }
		public void UpdateTransferStatus()
		{
			throw new NotImplementedException();
		}
	}
}