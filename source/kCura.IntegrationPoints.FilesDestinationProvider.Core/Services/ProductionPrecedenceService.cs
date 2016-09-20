using System.Collections.Generic;
using System.Data;
using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ProductionPrecedenceService : IProductionPrecedenceService
	{
		private readonly IConfig _config;
		private readonly ICredentialProvider _credentialProvider;

		public ProductionPrecedenceService(IConfig config, ICredentialProvider credentialProvider)
		{
			_config = config;
			_credentialProvider = credentialProvider;
		}

		public IEnumerable<ProductionPrecedenceDTO> GetProductionPrecedence(int workspaceArtifactID)
		{
			var productionManager = CreateProductionManager();

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

		private IProductionManager CreateProductionManager()
		{
			WinEDDS.Config.ProgrammaticServiceURL = _config.WebApiPath;

			var cookieContainer = new CookieContainer();
			var credentials = _credentialProvider.Authenticate(cookieContainer);

			var productionManager = new ProductionManagerFactory().Create(credentials, cookieContainer);

			return productionManager;
		}
	}
}