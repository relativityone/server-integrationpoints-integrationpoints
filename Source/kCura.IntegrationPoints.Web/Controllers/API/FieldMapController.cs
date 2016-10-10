using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
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
			var fieldsMap = _integrationPointReader.GetFieldMap(id).ToList();
			for(int index = 0; index < fieldsMap.Count; index++)
			{
				FieldMap fieldMap = fieldsMap[index];
				if (fieldMap.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
				{
					if (String.IsNullOrEmpty(fieldMap.DestinationField.FieldIdentifier))
					{
						fieldsMap.RemoveAt(index);
					}
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK, fieldsMap, Configuration.Formatters.JsonFormatter);
		}
	}
}
