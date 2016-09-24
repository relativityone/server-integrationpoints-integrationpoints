using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ProductionPrecedenceService : IProductionPrecedenceService
	{
		#region Fields

		private readonly IServiceManagerProvider _serviceManagerProvider;

		#endregion //Fields

		public ProductionPrecedenceService(IServiceManagerProvider serviceManagerProvider)
		{
			_serviceManagerProvider = serviceManagerProvider;
		}

		public IEnumerable<ProductionPrecedenceDTO> GetProductionPrecedence(int workspaceArtifactID)
		{
			var productionManager = _serviceManagerProvider.Create<IProductionManager, ProductionManagerFactory>();

			var dt = productionManager.RetrieveProducedByContextArtifactID(workspaceArtifactID).Tables[0];

			var result = new List<ProductionPrecedenceDTO>();
			foreach (DataRow row in dt.Rows)
				result.Add(new ProductionPrecedenceDTO
				{
					ArtifactID = row["ArtifactID"].ToString(),
					DisplayName = row["Name"].ToString()
				});

			return result;
		}
	}
}