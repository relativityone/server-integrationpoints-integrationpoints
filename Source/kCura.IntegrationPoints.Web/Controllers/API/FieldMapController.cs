using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class FieldMapController : ApiController
	{
		private readonly IIntegrationPointService _integrationPointReader;
		public FieldMapController(IIntegrationPointService integrationPointReader)
		{
			_integrationPointReader = integrationPointReader;
		}

		[LogApiExceptionFilter(Message = "Unable to retrieve fields mapping information.")]
		public HttpResponseMessage Get(int id)
		{
			List<FieldMap> fieldsMaps = _integrationPointReader.GetFieldMap(id).ToList();
			fieldsMaps.RemoveAll(
				fieldMap =>
					fieldMap.FieldMapType == FieldMapTypeEnum.FolderPathInformation &&
					string.IsNullOrEmpty(fieldMap.DestinationField.FieldIdentifier));

			return Request.CreateResponse(HttpStatusCode.OK, fieldsMaps, Configuration.Formatters.JsonFormatter);
		}
	}
}
