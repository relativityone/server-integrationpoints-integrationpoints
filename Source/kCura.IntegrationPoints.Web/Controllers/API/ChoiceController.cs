using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ChoiceController : ApiController
	{
		private readonly ChoiceService _choiceService;
		public ChoiceController(ChoiceService choiceService)
		{
			_choiceService = choiceService;
		}

		[HttpGet]
		[Route("{workspaceID}/Choice/{fieldGuid}")]
		[LogApiExceptionFilter(Message = "Unable to retrieve choice on field.")]
		public HttpResponseMessage Get(int workspaceID, string fieldGuid)
		{
			var choices = _choiceService.GetChoicesOnField(Guid.Parse(fieldGuid));
			
			return Request.CreateResponse(HttpStatusCode.OK, choices);
		}
	}
}
