using System.Data;

namespace kCura.IntegrationPoints.Domain.Readers
{
	public interface IDataTransferContext
	{
		bool HasDynamicRecordsCount { get; }
		IDataReader DataReader { get; set; }
		void UpdateTransferStatus();
	}

	public class DefaultTransferContext : IDataTransferContext
	{
		public DefaultTransferContext(IDataReader reader)
		{
			DataReader = reader;
		}

		public bool HasDynamicRecordsCount => false;
		public IDataReader DataReader { get; set; }
		public void UpdateTransferStatus()
		{
		}
	}
}