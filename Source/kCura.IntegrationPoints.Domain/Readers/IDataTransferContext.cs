using System.Data;

namespace kCura.IntegrationPoints.Domain.Readers
{
	public interface IDataTransferContext
	{
		IDataReader DataReader { get; set; }
		int? TotalItemsFound { get; set; }
		int TransferredItemsCount { get; set; }
		int FailedItemsCount { get; set; }
		void UpdateTransferStatus();
	}
}