using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Helpers;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class SummaryPageController : Controller
    {
        private readonly ICaseServiceContext _context;
        private readonly IProviderTypeService _providerTypeService;
        private readonly SummaryPageSelector _summaryPageSelector;
        private readonly IDocumentAccumulatedStatistics _documentAccumulatedStatistics;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly ICalculationChecker _calculationChecker;

        public SummaryPageController(
            ICaseServiceContext context,
            IProviderTypeService providerTypeService,
            SummaryPageSelector summaryPageSelector,
            IDocumentAccumulatedStatistics documentAccumulatedStatistics,
            IIntegrationPointService integrationPointService,
            IIntegrationPointProfileService integrationPointProfileService)
            IDocumentAccumulatedStatistics documentAccumulatedStatistics,
            ICalculationChecker calculationChecker)
        {
            _context = context;
            _providerTypeService = providerTypeService;
            _summaryPageSelector = summaryPageSelector;
            _documentAccumulatedStatistics = documentAccumulatedStatistics;
            _integrationPointService = integrationPointService;
            _integrationPointProfileService = integrationPointProfileService;
            _calculationChecker = calculationChecker;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to get SummaryPage for requested Integration Point")]
        public ActionResult GetSummaryPage(int id, string controllerType)
        {
            Tuple<int, int> providerIds = GetSourceAndDestinationProviderIdsAsync(id, controllerType);

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

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to get CalculationState info from IntegrationPoint")]
        public async Task<ActionResult> GetCalculationStateInfo(int integrationPointId)
        {
            CalculationState calculationState = await _calculationChecker.GetCalculationState(integrationPointId).ConfigureAwait(false);
            return Json(calculationState);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to get natives statistics for saved search")]
        public async Task<ActionResult> GetNativesStatisticsForSavedSearch(int workspaceId, int savedSearchId, int integrationPointId)
        {
            CalculationState calculationState = await _calculationChecker.MarkAsCalculating(integrationPointId).ConfigureAwait(false);
            if (calculationState.Status != CalculationStatus.Error)
            {
                DocumentsStatistics result = await _documentAccumulatedStatistics.GetNativesStatisticsForSavedSearchAsync(workspaceId, savedSearchId).ConfigureAwait(false);
                calculationState = await _calculationChecker.MarkCalculationAsFinished(integrationPointId, result).ConfigureAwait(false);
            }

            return Json(calculationState);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to get images statistics for saved search")]
        public async Task<ActionResult> GetImagesStatisticsForSavedSearch(int workspaceId, int savedSearchId, bool calculateSize, int integrationPointId)
        {
            CalculationState calculationState = await _calculationChecker.MarkAsCalculating(integrationPointId).ConfigureAwait(false);
            if (calculationState.Status != CalculationStatus.Error)
            {
                DocumentsStatistics result = await _documentAccumulatedStatistics.GetImagesStatisticsForSavedSearchAsync(workspaceId, savedSearchId, calculateSize)
               .ConfigureAwait(false);
                calculationState = await _calculationChecker.MarkCalculationAsFinished(integrationPointId, result).ConfigureAwait(false);
            }

            return Json(calculationState);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to get images statistics for production")]
        public async Task<ActionResult> GetImagesStatisticsForProduction(int workspaceId, int productionId, int integrationPointId)
        {
            CalculationState calculationState = await _calculationChecker.MarkAsCalculating(integrationPointId).ConfigureAwait(false);
            if (calculationState.Status != CalculationStatus.Error)
            {
                DocumentsStatistics result = await _documentAccumulatedStatistics.GetImagesStatisticsForProductionAsync(workspaceId, productionId)
                .ConfigureAwait(false);
                calculationState = await _calculationChecker.MarkCalculationAsFinished(integrationPointId, result).ConfigureAwait(false);
            }

            return Json(calculationState);
        }

        private Tuple<int, int> GetSourceAndDestinationProviderIdsAsync(int integrationPointId, string controllerType)
        {
            if (controllerType == IntegrationPointApiControllerNames.IntegrationPointApiControllerName)
            {
                IntegrationPointSlimDto integrationPoint = _integrationPointService.ReadSlim(integrationPointId);

                return new Tuple<int, int>(integrationPoint.SourceProvider, integrationPoint.DestinationProvider);
            }
            else
            {
                IntegrationPointProfileSlimDto integrationPointProfile = _integrationPointProfileService.ReadSlim(integrationPointId);

                return new Tuple<int, int>(integrationPointProfile.SourceProvider, integrationPointProfile.DestinationProvider);
            }
        }
    }

    public static class IntegrationPointApiControllerNames
    {
        public static string IntegrationPointApiControllerName => "IntegrationPointsAPI";

        public static string IntegrationPointProfileApiControllerName => "IntegrationPointProfilesAPI";
    }
}
