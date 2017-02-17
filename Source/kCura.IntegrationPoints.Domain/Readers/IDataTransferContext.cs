using System.Data;

namespace kCura.IntegrationPoints.Domain.Readers
{
	public interface IDataTransferContext
	{
		IDataReader DataReader { get; set; }
		int? TotalItemsFound { get; set; }
		void UpdateTransferStatus();
	}

	public class DefaultTransferContext : IDataTransferContext
	{
		public DefaultTransferContext(IDataReader reader)
		{
			DataReader = reader;
		}
		
		public IDataReader DataReader { get; set; }
		public int? TotalItemsFound { get; set; }

		public void UpdateTransferStatus()
		{
		}
	}
}