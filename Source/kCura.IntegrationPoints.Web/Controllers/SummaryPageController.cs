using System;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Helpers;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class SummaryPageController : BaseController
	{
		private readonly ICaseServiceContext _context;
		private readonly IProviderTypeService _providerTypeService;
		private readonly SummaryPageSelector _summaryPageSelector;

		public SummaryPageController(ICaseServiceContext context, IProviderTypeService providerTypeService, SummaryPageSelector summaryPageSelector)
		{
			_context = context;
			_providerTypeService = providerTypeService;
			_summaryPageSelector = summaryPageSelector;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to get SummaryPage for requested Integration Point")]
		public ActionResult GetSummaryPage(int id, string controllerType)
		{
			Tuple<int, int> providerIds = GetSourceAndDestinationProviderIds(id, controllerType);

			ProviderType providerType = _providerTypeService.GetProviderType(providerIds.Item1, providerIds.Item2);

			string customPagePath = _summaryPageSelector[providerType];

			return PartialView(customPagePath);
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to get scheduling details")]
		public ActionResult SchedulerSummaryPage()
		{
			return PartialView("~/Views/IntegrationPoints/SchedulerSummaryPage.cshtml");
		}

		private Tuple<int, int> GetSourceAndDestinationProviderIds(int integrationPointId, string controllerType)
		{
			if (controllerType == IntegrationPointApiControllerNames.IntegrationPointApiControllerName)
			{
				IntegrationPoint integrationPoint = _context.RsapiService.IntegrationPointLibrary.Read(integrationPointId);

				return new Tuple<int, int>(integrationPoint.SourceProvider.Value, integrationPoint.DestinationProvider.Value);
			}
			else
			{
				IntegrationPointProfile integrationPointProfile =
					_context.RsapiService.IntegrationPointProfileLibrary.Read(integrationPointId);

				return new Tuple<int, int>(integrationPointProfile.SourceProvider.Value,
					integrationPointProfile.DestinationProvider.Value);
			}
		}
	}

	public static class IntegrationPointApiControllerNames
	{
		public static string IntegrationPointApiControllerName => "IntegrationPointsAPI";
		public static string IntegrationPointProfileApiControllerName => "IntegrationPointProfilesAPI";
	}
}