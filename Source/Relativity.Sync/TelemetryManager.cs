using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync
{
	internal sealed class TelemetryManager
	{
		private const string _MY_NEW_CATEGORY_NAME = "Relativity.Sync";

		private readonly IServicesMgr _servicesManager;
		private readonly List<ITelemetryMetricProvider> _metricProviders;
		public TelemetryManager(IServicesMgr servicesManager)
		{
			_servicesManager = servicesManager;
			_metricProviders = new List<ITelemetryMetricProvider>();
		}

		public void AddMetricProviders(ITelemetryMetricProvider metricProvider)
		{
			if (metricProvider == null)
			{
				LogEmptyMetricProvider();
				throw new ArgumentNullException(nameof(metricProvider));
			}
			_metricProviders.Add(metricProvider);
		}

		public async Task InstallMetrics()
		{
			CategoryRef category = await CreateAndEnableMetricCategoryIfNotExistAsync(_MY_NEW_CATEGORY_NAME).ConfigureAwait(false);

			_metricProviders.ForEach(item => item.AddMetricsForCategory(category));
		}

		private async Task<CategoryRef> CreateAndEnableMetricCategoryIfNotExistAsync(string categoryName)
		{
			using (var manager = _servicesManager.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
			{
				List<CategoryTarget> categories = await manager.GetCategoryTargetsAsync().ConfigureAwait(false);
				CategoryTarget newCategory = categories.FirstOrDefault(categoryTarget => categoryTarget.Category.Name.Equals(_MY_NEW_CATEGORY_NAME, StringComparison.OrdinalIgnoreCase));
				CategoryRef categoryRef = newCategory?.Category;

				if (categoryRef == null)
				{
					categoryRef = await AddCategoryAsync(categoryName, manager).ConfigureAwait(false);
					await EnableMetricsForCategoryAsync(categoryRef, manager).ConfigureAwait(false);
				}

				return categoryRef;
			}
		}

		private async Task<CategoryRef> AddCategoryAsync(string categoryName, IInternalMetricsCollectionManager manager)
		{
			var category = new Category
			{
				Name = categoryName
			};

			category.ID = await manager.CreateCategoryAsync(category, false).ConfigureAwait(false);
			return category;
		}

		private async Task EnableMetricsForCategoryAsync(CategoryRef category, IInternalMetricsCollectionManager manager)
		{
			CategoryTarget categoryTarget = new CategoryTarget
			{
				Category = category,
				IsCategoryMetricTargetEnabled = new Dictionary<CategoryMetricTarget, bool> { { CategoryMetricTarget.SUM, true } }
			};

			await manager.UpdateCategoryTargetSingleAsync(categoryTarget).ConfigureAwait(false);
		}

		private void LogEmptyMetricProvider()
		{
			throw new NotImplementedException();
		}
	}
}