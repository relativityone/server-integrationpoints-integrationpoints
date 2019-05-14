using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync.Telemetry
{
	internal sealed class TelemetryManager : ITelemetryManager
	{
		private readonly IServicesMgr _servicesManager;
		private readonly ISyncLog _logger;
		private readonly List<ITelemetryMetricProvider> _metricProviders;

		public TelemetryManager(IServicesMgr servicesManager, ISyncLog logger)
		{
			_servicesManager = servicesManager;
			_logger = logger;
			_metricProviders = new List<ITelemetryMetricProvider>();
		}

		public void AddMetricProviders(ITelemetryMetricProvider metricProvider)
		{
			if (metricProvider == null)
			{
				var exception = new ArgumentNullException(nameof(metricProvider));
				_logger.LogDebug(exception, "Metric provider shouldn't be null.");
			}
			_metricProviders.Add(metricProvider);
		}

		public async Task InstallMetrics()
		{
			try
			{
				CategoryRef category = await CreateAndEnableMetricCategoryIfNotExistAsync(TelemetryConstants.TELEMETRY_CATEGORY).ConfigureAwait(false);

				_metricProviders.ForEach(item => item.AddMetricsForCategory(category));
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error occured when installing SUM metrics. SUM metrics might not be logged.");
			}
		}

		private async Task<CategoryRef> CreateAndEnableMetricCategoryIfNotExistAsync(string categoryName)
		{
			using (var manager = _servicesManager.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
			{
				List<CategoryTarget> categories = await manager.GetCategoryTargetsAsync().ConfigureAwait(false);
				CategoryTarget newCategory = categories.FirstOrDefault(categoryTarget => categoryTarget.Category.Name.Equals(TelemetryConstants.TELEMETRY_CATEGORY, StringComparison.OrdinalIgnoreCase));
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
	}
}