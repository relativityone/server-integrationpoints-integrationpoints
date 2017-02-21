using System.Data;

namespace kCura.IntegrationPoints.Domain.Readers
{
	public interface IDataTransferContext
	{
		IDataReader DataReader { get; set; }
		int? TotalItemsFound { get; set; }
		void UpdateTransferStatus();
	}
}