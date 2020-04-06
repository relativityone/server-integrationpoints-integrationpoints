using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync.Telemetry
{
	internal sealed class TelemetryMetricsInstaller : ITelemetryManager
	{
		private readonly ISyncServiceManager _servicesManager;
		private readonly ISyncLog _logger;
		private readonly List<ITelemetryMetricProvider> _metricProviders;

		public TelemetryMetricsInstaller(ISyncServiceManager servicesManager, ISyncLog logger)
		{
			_servicesManager = servicesManager;
			_logger = logger;
			_metricProviders = new List<ITelemetryMetricProvider>();
		}

		public void AddMetricProvider(ITelemetryMetricProvider metricProvider)
		{
			if (metricProvider == null)
			{
				var exception = new ArgumentNullException(nameof(metricProvider));
				_logger.LogDebug(exception, "Metric provider shouldn't be null.");
			}
			else
			{
				_metricProviders.Add(metricProvider);
			}
		}

		public async Task InstallMetrics()
		{
			try
			{
				IDictionary<string, CategoryRef> categories = await CreateAndEnableMetricCategoryIfNotExistAsync(_metricProviders.Select(item => item.CategoryName))
					.ConfigureAwait(false);

				AddMetricsForCategories(categories);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error occured when installing SUM metrics. SUM metrics might not be logged.");
			}
		}

		private void AddMetricsForCategories(IDictionary<string, CategoryRef> categories)
		{
			using (var manager = _servicesManager.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
			{
				_metricProviders.ForEach(item => item.AddMetricsForCategory(manager, categories[item.CategoryName]).GetAwaiter().GetResult());
			}
		}

		private async Task<IDictionary<string, CategoryRef>> CreateAndEnableMetricCategoryIfNotExistAsync(IEnumerable<string> categoriesNames)
		{
			IDictionary<string, CategoryRef> categories = new Dictionary<string, CategoryRef>();

			using (var manager = _servicesManager.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
			{
				List<CategoryTarget> categoryTargets = await manager.GetCategoryTargetsAsync().ConfigureAwait(false);

				foreach (string categoryName in categoriesNames)
				{
					CategoryRef categoryRef = categoryTargets.FirstOrDefault(categoryTarget => categoryTarget.Category.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))?.Category;

					if (categoryRef is null)
					{
						categoryRef = await AddCategoryAsync(categoryName, manager).ConfigureAwait(false);
						await EnableMetricsForCategoryAsync(categoryRef, manager).ConfigureAwait(false);
					}

					categories.Add(categoryName, categoryRef);
				}

				return categories;
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