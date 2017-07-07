using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SourceOptions
	{
		public Guid Type { get; set; }
		public object Options { get; set; }
	}

	public class SourceFieldsController : ApiController
	{
		private readonly IGetSourceProviderRdoByIdentifier _sourceProviderIdentifier;
		private readonly ICaseServiceContext _caseService;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IManagerFactory _managerFactory;
		private readonly IDataProviderFactory _factory;
		private readonly IHelper _helper;

		public SourceFieldsController(
			IGetSourceProviderRdoByIdentifier sourceProviderIdentifier,
			ICaseServiceContext caseService,
			IContextContainerFactory contextContainerFactory,
			IManagerFactory managerFactory,
			IDataProviderFactory factory, 
			IHelper helper)
		{
			_sourceProviderIdentifier = sourceProviderIdentifier;
			_caseService = caseService;
			_contextContainerFactory = contextContainerFactory;
			_managerFactory = managerFactory;
			_factory = factory;
			_helper = helper;

		}

		[HttpPost]
		[Route("{workspaceID}/api/SourceFields/")]
		[LogApiExceptionFilter(Message = "Unable to retrieve source fields.")]
		public HttpResponseMessage Get(SourceOptions data)
		{
			Data.SourceProvider providerRdo = _sourceProviderIdentifier.Execute(data.Type);
			Guid applicationGuid = new Guid(providerRdo.ApplicationIdentifier);
			var provider = _factory.GetDataProvider(applicationGuid, data.Type, _helper);
			List<FieldEntry> fields = provider.GetFields(data.Options.ToString()).OrderBy(x => x.DisplayName).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, fields, Configuration.Formatters.JsonFormatter);
		}
	}
}
