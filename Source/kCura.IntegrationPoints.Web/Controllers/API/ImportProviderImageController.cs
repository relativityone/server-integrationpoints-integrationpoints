using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ImportProviderImageController : ApiController
    {

		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly ICPHelper _helper;

		public ImportProviderImageController(IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, ICPHelper helper)
		{
			_managerFactory = managerFactory;
			_contextContainerFactory = contextContainerFactory;
			_helper = helper;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve list of Overlay Identifier values.")]
		public IHttpActionResult GetOverlayIdentifierFields(int workspaceArtifactId)
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IFieldManager fieldManager = _managerFactory.CreateFieldManager(contextContainer);
			
			ArtifactFieldDTO[] fieldResults = fieldManager.RetrieveBeginBatesFields(workspaceArtifactId);
			return Json(fieldResults);
		}
	}
}
