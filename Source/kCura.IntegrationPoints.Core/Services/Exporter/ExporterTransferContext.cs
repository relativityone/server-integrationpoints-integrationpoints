using System.Data;
using kCura.IntegrationPoints.Domain.Readers;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class ExporterTransferContext : IDataTransferContext
	{
		private readonly IExporterTransferConfiguration _configuration;

		public ExporterTransferContext(IDataReader  reader,IExporterTransferConfiguration configuration)
		{
			_configuration = configuration;
			DataReader = reader;
		}
		
		public IDataReader DataReader { get; set; }
		public int? TotalItemsFound { get; set; }

		public void UpdateTransferStatus()
		{
			if (TotalItemsFound.HasValue)
			{
				Data.JobHistory dto = _configuration.JobHistoryService.GetRdo(_configuration.Identifier);
				dto.TotalItems = TotalItemsFound;
				_configuration.JobHistoryService.UpdateRdo(dto);
			}
		}
	}
}