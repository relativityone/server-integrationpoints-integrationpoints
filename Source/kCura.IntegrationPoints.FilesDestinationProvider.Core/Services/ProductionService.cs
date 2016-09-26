using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ProductionService : IProductionService
	{
		#region Fields

		private readonly IServiceManagerProvider _serviceManagerProvider;

		#endregion //Fields

		public ProductionService(IServiceManagerProvider serviceManagerProvider)
		{
			_serviceManagerProvider = serviceManagerProvider;
		}

		public IEnumerable<ProductionDTO> GetProductions(int workspaceArtifactID)
		{
			var productionManager = _serviceManagerProvider.Create<IProductionManager, ProductionManagerFactory>();

			var dt = productionManager.RetrieveProducedByContextArtifactID(workspaceArtifactID).Tables[0];

			var result = new List<ProductionDTO>();
			foreach (DataRow row in dt.Rows)
				result.Add(new ProductionDTO
				{
					ArtifactID = row["ArtifactID"].ToString(),
					DisplayName = row["Name"].ToString()
				});

			return result;
		}
	}
}