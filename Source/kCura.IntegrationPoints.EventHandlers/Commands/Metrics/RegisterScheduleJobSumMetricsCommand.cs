using kCura.IntegrationPoints.Core.Telemetry;
using Polly;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Metrics
{
    public class RegisterScheduleJobSumMetricsCommand : IEHCommand
    {
        private readonly IEHHelper _helper;
        private readonly IAPILog _log;

        public RegisterScheduleJobSumMetricsCommand(IEHHelper helper)
        {
            _helper = helper;
            _log = _helper.GetLoggerFactory().GetLogger().ForContext<RegisterScheduleJobSumMetricsCommand>();
        }

        public void Execute()
        {
            try
            {
                const int maxRetryTimes = 3;
                var retryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(maxRetryTimes,
                        i => TimeSpan.FromSeconds(Math.Pow(3, i)),
                        (exception, timeSpan, retryCount, _) => _log.LogError(exception, "Action failed for {nth} time.", retryCount));

                retryPolicy.ExecuteAsync(async () => await RegisterCategoryAndMetrics().ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult();

                _log.LogInformation("Sync Schedule metrics installed");
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error occurred when installing SUM metrics. SUM metrics might not be logged.");
            }
        }

        private async Task RegisterCategoryAndMetrics()
        {
            CategoryRef category = await CreateAndEnableMetricCategoryIfNotExistAsync(MetricsBucket.SyncSchedule.SYNC_SCHEDULE_CATEGORY).ConfigureAwait(false);

            await AddMetricsForCategoryAsync(category).ConfigureAwait(false);

            _log.LogInformation("Integration Points Sync Schedule metrics installed.");
        }

        private async Task<CategoryRef> CreateAndEnableMetricCategoryIfNotExistAsync(string categoryName)
        {
            using (var manager = _helper.GetServicesManager().CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
            {
                List<CategoryTarget> categories = await manager.GetCategoryTargetsAsync().ConfigureAwait(false);
                CategoryTarget newCategory = categories.FirstOrDefault(categoryTarget => categoryTarget.Category.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                CategoryRef categoryRef = newCategory?.Category;

                if (categoryRef == null)
                {
                    categoryRef = await AddCategoryAsync(categoryName, manager).ConfigureAwait(false);
                    await EnableMetricsForCategoryAsync(categoryRef, manager).ConfigureAwait(false);
                }

                return categoryRef;
            }
        }

        private static async Task<CategoryRef> AddCategoryAsync(string categoryName, IInternalMetricsCollectionManager manager)
        {
            var category = new Category
            {
                Name = categoryName
            };

            category.ID = await manager.CreateCategoryAsync(category, false).ConfigureAwait(false);
            return category;
        }

        private static async Task EnableMetricsForCategoryAsync(CategoryRef category, IInternalMetricsCollectionManager manager)
        {
            CategoryTarget categoryTarget = new CategoryTarget
            {
                Category = category,
                IsCategoryMetricTargetEnabled = new Dictionary<CategoryMetricTarget, bool> { { CategoryMetricTarget.SUM, true } }
            };

            await manager.UpdateCategoryTargetSingleAsync(categoryTarget).ConfigureAwait(false);
        }

        private async Task AddMetricsForCategoryAsync(CategoryRef category)
        {
            try
            {
                using (var manager = _helper.GetServicesManager().CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
                {
                    foreach (MetricIdentifier metricIdentifier in MetricsBucket.SyncSchedule.METRICS)
                    {
                        if (metricIdentifier.Categories != null)
                        {
                            metricIdentifier.Categories.Add(category);
                        }
                        else
                        {
                            metricIdentifier.Categories = new List<CategoryRef> { category };
                        }

                        await manager.CreateMetricIdentifierAsync(metricIdentifier, false).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Exception occurred when adding metrics to category {categoryName}.", category.Name);
                throw;
            }
        }
    }
}
