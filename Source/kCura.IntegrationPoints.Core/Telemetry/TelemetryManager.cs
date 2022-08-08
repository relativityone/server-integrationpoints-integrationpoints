using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Telemetry
{
    public class TelemetryManager
    {
        #region Constructors

        public TelemetryManager(IHelper helper)
        {
            _helper = helper;
            _logger = _helper.GetLoggerFactory().GetLogger().ForContext<TelemetryManager>();
        }

        #endregion //Constructors

        #region Fields

        private readonly IHelper _helper;
        private readonly IAPILog _logger;
        private readonly List<ITelemetryMetricProvider> _metricProviders = new List<ITelemetryMetricProvider>();

        #endregion //Fields

        #region Methods

        public void AddMetricProviders(ITelemetryMetricProvider metricProvider)
        {
            if (metricProvider == null)
            {
                LogEmptyMetricProvider();
                throw new ArgumentNullException(nameof(metricProvider));
            }
            _metricProviders.Add(metricProvider);
        }

        public void InstallMetrics()
        {
            Category category = AddCategory(Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY);
            EnableMetrics(category);
            AddProvidersMetricIdentifiers(category);
        }

        private void AddProvidersMetricIdentifiers(Category category)
        {
            _metricProviders.ForEach(item => item.Run(category, _helper));
        }

        private void EnableMetrics(Category category)
        {
            try
            {
                using (
                    IInternalMetricsCollectionManager internalMetricsCollectionManager =
                        _helper.GetServicesManager().CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
                {
                    List<CategoryTarget> targets = Task.Run(async () => await internalMetricsCollectionManager.GetCategoryTargetsAsync()).ConfigureAwait(false).GetAwaiter().GetResult();
                    CategoryTarget ipCategoryTarget = targets.FirstOrDefault(x => x.Category.Name == category.Name);

                    if (ipCategoryTarget == null)
                    {
                        LogMissingCategoryTarget(category);
                        throw new Exception($"Cannot find category target for telemetry category ${category.Name}");
                    }
                    ipCategoryTarget.IsCategoryMetricTargetEnabled[CategoryMetricTarget.APM] = true;
                    ipCategoryTarget.IsCategoryMetricTargetEnabled[CategoryMetricTarget.SUM] = true;

                    Task.Run(async () => await internalMetricsCollectionManager.UpdateCategoryTargetsAsync(targets)).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (AggregateException ex)
            {
                LogSettingTargetCategoryError(category);
                throw new Exception($"Failed to set target category metrics for telemetry category {category.Name}", ex.InnerException);
            }
        }

        private Category AddCategory(string categoryName)
        {
            try
            {
                using (IInternalMetricsCollectionManager internalMetricsCollectionManager =
                    _helper.GetServicesManager().CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
                {
                    var category = new Category
                    {
                        Name = categoryName
                    };

                    category.ID =
                        Task.Run(async () => await internalMetricsCollectionManager.CreateCategoryAsync(category, false))
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();

                    return category;
                }
            }
            catch (AggregateException ex)
            {
                LogAddingTelemetryCategoryError(categoryName, ex);
                throw new Exception($"Failed to add telemetry category {categoryName}", ex.InnerException);
            }
        }

        #endregion //Methods

        #region Logging

        private void LogEmptyMetricProvider()
        {
            _logger.LogError("Metric provider cannot be null.");
        }

        private void LogSettingTargetCategoryError(Category category)
        {
            _logger.LogError("Failed to set target category metrics for telemetry category {CategoryName}.", category.Name);
        }

        private void LogMissingCategoryTarget(Category category)
        {
            _logger.LogError("Cannot find category target for telemetry category {CategoryName}", category.Name);
        }

        private void LogAddingTelemetryCategoryError(string categoryName, AggregateException ex)
        {
            _logger.LogError(ex, "Failed to add telemetry category {CategoryName}.", categoryName);
        }

        #endregion
    }
}