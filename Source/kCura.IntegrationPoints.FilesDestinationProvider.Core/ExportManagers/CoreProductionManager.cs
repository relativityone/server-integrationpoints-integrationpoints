using System.Data;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using System.Linq;
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
		public DataSet RetrieveImportEligibleByContextArtifactID(int caseContextArtifactID)
		{
			return InitProductionManager(caseContextArtifactID).ExternalRetrieveImportEligible(_baseServiceContext);
		}

		public object[][] RetrieveBatesByProductionAndDocument(int caseContextArtifactID, int[] productionIds, int[] documentIds)
		{
			object[][] retBegBatesInfo = global::Relativity.Core.Service.ProductionQuery.RetrieveBatesByProductionAndDocument(
				_baseServiceContext, _userPermissionsMatrix, productionIds, documentIds)
				.Table
				.Select()
				.Select( dr => global::Relativity.Export.ProductionDocumentBatesHelper.ToSerializableObjectArray(dr)).ToArray();

			global::Relativity.Export.ProductionDocumentBatesHelper.CleanupSerialization(retBegBatesInfo);
			return retBegBatesInfo;
		}

		private ProductionManager InitProductionManager(int appArtifactId)
		{
			_baseServiceContext.AppArtifactID = appArtifactId;
			return new ProductionManager();
		}
	}
}