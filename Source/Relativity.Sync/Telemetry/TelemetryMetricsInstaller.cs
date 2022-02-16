using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.InternalMetricsCollection;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Telemetry
{
	internal sealed class TelemetryMetricsInstaller : ITelemetryManager
	{
		private readonly ISourceServiceFactoryForAdmin _servicesManager;
		private readonly ISyncLog _logger;
		private readonly List<ITelemetryMetricProvider> _metricProviders;

		public TelemetryMetricsInstaller(ISourceServiceFactoryForAdmin servicesManager, ISyncLog logger)
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
				IDictionary<string, CategoryRef> categories = await CreateAndEnableMetricCategoriesIfNotExistAsync(_metricProviders.Select(item => item.CategoryName))
					.ConfigureAwait(false);

				AddMetricsForCategories(categories);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error occurred when installing SUM metrics. SUM metrics might not be logged.");
			}
		}

		private void AddMetricsForCategories(IDictionary<string, CategoryRef> categories)
		{
			using (var manager = _servicesManager.CreateProxyAsync<IInternalMetricsCollectionManager>().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				_metricProviders.ForEach(item => item.AddMetricsForCategory(manager, categories[item.CategoryName]).GetAwaiter().GetResult());
			}
		}

		private async Task<IDictionary<string, CategoryRef>> CreateAndEnableMetricCategoriesIfNotExistAsync(IEnumerable<string> categoriesNames)
		{
			IDictionary<string, CategoryRef> categories = new Dictionary<string, CategoryRef>();

			using (var manager = _servicesManager.CreateProxyAsync<IInternalMetricsCollectionManager>().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				List<CategoryTarget> categoryTargets = await manager.GetCategoryTargetsAsync().ConfigureAwait(false);

				foreach (string categoryName in categoriesNames)
				{
					CategoryRef categoryRef = categoryTargets.FirstOrDefault(categoryTarget => categoryTarget.Category.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))?.Category;

					if (categoryRef is null)
					{
						categoryRef = await AddCategoryAsync(categoryName, manager).ConfigureAwait(false);
					}

					categories.Add(categoryName, categoryRef);
				}

				return categories;
			}
		}

		private static async Task<CategoryRef> AddCategoryAsync(string categoryName, IInternalMetricsCollectionManager manager)
		{
			var categoryRef = new Category
			{
				Name = categoryName
			};

			categoryRef.ID = await manager.CreateCategoryAsync(categoryRef, false).ConfigureAwait(false);

			await EnableSumMetricTargetForCategoryAsync(categoryRef, manager).ConfigureAwait(false);

			return categoryRef;
		}

		private static async Task EnableSumMetricTargetForCategoryAsync(CategoryRef category, IInternalMetricsCollectionManager manager)
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