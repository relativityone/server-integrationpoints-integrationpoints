using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.Services.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ViewFinderController : ApiController
	{
		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve view list.")]
		public HttpResponseMessage Get(int workspaceId, string search, int page = 1)
		{
			List<ViewModel> views = new List<ViewModel>()
			{
				new ViewModel()
				{
					DisplayName = "My View",
					Value = 1
				}
			};

			var response = new ViewResultsModel
			{
				Results = views,
				TotalResults = views.Count,
				HasMoreResults = false
			};

			return Request.CreateResponse(HttpStatusCode.OK, response);
		}
	}
}
