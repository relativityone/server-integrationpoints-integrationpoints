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
		#region Fields

		private readonly IHelper _helper;
		private readonly List<ITelemetryMetricProvider> _metricProviders = new List<ITelemetryMetricProvider>();

		#endregion //Fields

		#region Constructors

		public TelemetryManager(IHelper helper)
		{
			_helper = helper;
		}

		#endregion //Constructors

		#region Methods

		public void AddMetricProviders(ITelemetryMetricProvider metricProvider)
		{
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
				using (IInternalMetricsCollectionManager internalMetricsCollectionManager = _helper.GetServicesManager().CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
				{
					List<CategoryTarget> targets = Task.Run(async () => await internalMetricsCollectionManager.GetCategoryTargetsAsync()).ConfigureAwait(false).GetAwaiter().GetResult();
					CategoryTarget ipCategoryTarget = targets.FirstOrDefault(x => x.Category.Name == Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY);

					ipCategoryTarget.IsCategoryMetricTargetEnabled[CategoryMetricTarget.APM] = true;
					ipCategoryTarget.IsCategoryMetricTargetEnabled[CategoryMetricTarget.SUM] = true;

					Task.Run(async () => await internalMetricsCollectionManager.UpdateCategoryTargetsAsync(targets)).ConfigureAwait(false).GetAwaiter().GetResult();
				}
			}
			catch (AggregateException ex)
			{
				throw new Exception($"Failed to set target category metrics for telemetry category {Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY}", ex.InnerException);
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
				throw new Exception($"Failed to add telemetry category {Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY}", ex.InnerException);
			}
		}

		#endregion //Methods
	}
}
