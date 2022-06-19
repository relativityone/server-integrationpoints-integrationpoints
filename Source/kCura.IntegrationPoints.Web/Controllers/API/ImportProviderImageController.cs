using System.Linq;
using System.Web.Http;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Factories;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ImportProviderImageController : ApiController
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IManagerFactory _managerFactory;

		public ImportProviderImageController(IManagerFactory managerFactory, IServicesMgr servicesMgr)
		{
			_managerFactory = managerFactory;
			_servicesMgr = servicesMgr;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve list of Overlay Identifier values.")]
		public IHttpActionResult GetOverlayIdentifierFields(int workspaceArtifactId)
		{
			Core.Managers.IFieldManager fieldManager = _managerFactory.CreateFieldManager();

			ArtifactFieldDTO[] fieldResults = fieldManager.RetrieveBeginBatesFields(workspaceArtifactId);
			return Json(fieldResults);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve list of file repository values.")]
		public async Task<IHttpActionResult> GetFileRepositories(int workspaceArtifactId)
		{
			string[] folderPathsForCase = null;
			using (ICaseService caseService = _servicesMgr.CreateProxy<ICaseService>(ExecutionIdentity.System))
			{
				folderPathsForCase = await caseService.GetAllDocumentFolderPathsForCaseAsync(workspaceArtifactId, string.Empty).ConfigureAwait(false);
			}

			return Json(folderPathsForCase.OrderBy(x => x).ToArray());
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve the default file repo.")]
		public async Task<IHttpActionResult> GetDefaultFileRepo(int workspaceArtifactId)
		{
			CaseInfo caseInfo = null;
			using (ICaseService caseService = _servicesMgr.CreateProxy<ICaseService>(ExecutionIdentity.System))
			{
				caseInfo = await caseService.ReadAsync(workspaceArtifactId, string.Empty).ConfigureAwait(false);
			}

			return Json(caseInfo.DocumentPath);
		}
	}
}
