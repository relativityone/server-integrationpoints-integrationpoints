namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	using System.Collections.Generic;
	using Core.Models;
	using Core.Services.IntegrationPoint;
	using Data;

	public abstract class UpdateIntegrationPointConfigurationCommandBase : IEHCommand
	{
		public IIntegrationPointForSourceService IntegrationPointForSourceService { get; }
		public IIntegrationPointService IntegrationPointService { get; }

		protected abstract string SourceProviderGuid { get; }

		protected UpdateIntegrationPointConfigurationCommandBase(IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService)
		{
			IntegrationPointForSourceService = integrationPointForSourceService;
			IntegrationPointService = integrationPointService;
		}

		public virtual void Execute()
		{
			IList<IntegrationPoint> selectedIntegrationPoints = IntegrationPointForSourceService.GetAllForSourceProvider(SourceProviderGuid);

			foreach (IntegrationPoint integrationPoint in selectedIntegrationPoints)
			{
				IntegrationPoint processedIntegrationPoint = ConvertIntegrationPoint(integrationPoint);
				if (processedIntegrationPoint != null)
				{
					IntegrationPointService.SaveIntegration(IntegrationPointModel.FromIntegrationPoint(integrationPoint));
				}
			}
		}

		/// <summary>
		/// Process and convert existing integration point. The returned value is saved back to the database. No save is performed if null is returned.
		/// </summary>
		protected abstract IntegrationPoint ConvertIntegrationPoint(IntegrationPoint integrationPoint);
	}
}