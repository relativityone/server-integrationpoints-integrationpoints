using System.Data;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using ProductionManager = Relativity.Core.Service.ProductionManager;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreProductionManager : IProductionManager
	{
		private readonly BaseServiceContext _baseServiceContext;
		private readonly UserPermissionsMatrix _userPermissionsMatrix;

		public CoreProductionManager(BaseServiceContext baseServiceContext)
		{
			_baseServiceContext = baseServiceContext;
			_userPermissionsMatrix = new UserPermissionsMatrix(_baseServiceContext);
		}

		public ProductionInfo Read(int caseContextArtifactID, int productionArtifactID)
		{
			return InitProductionManager(caseContextArtifactID).ReadInfo(_baseServiceContext, productionArtifactID).ToProductionInfo();
		}

		public DataSet RetrieveProducedByContextArtifactID(int caseContextArtifactID)
		{
			return InitProductionManager(caseContextArtifactID).ExternalRetrieveProduced(_baseServiceContext, _userPermissionsMatrix);
		}

		private ProductionManager InitProductionManager(int appArtifactId)
		{
			_baseServiceContext.AppArtifactID = appArtifactId;
			return new ProductionManager();
		}
	}
}