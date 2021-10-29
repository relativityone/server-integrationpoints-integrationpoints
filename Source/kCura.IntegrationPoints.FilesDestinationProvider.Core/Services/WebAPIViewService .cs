using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class WebAPIViewService : IViewService
	{
		#region Constructors

		public WebAPIViewService(IServiceManagerProvider serviceManagerProvider, IHelper helper)
		{
			_serviceManagerProvider = serviceManagerProvider;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ViewService>();
		}

		#endregion //Constructors

		#region Fields

		private readonly IServiceManagerProvider _serviceManagerProvider;
		private readonly IAPILog _logger;

		#endregion //Fields

		#region Methods

		public List<ViewDTO> GetViewsByWorkspaceAndArtifactType(int workspceId, int artifactTypeId)
		{
			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();
			// Third argument has to be always False in case of RIP Export
			DataTableCollection retTables = searchManager.RetrieveViewsByContextArtifactID(workspceId, artifactTypeId, false).Tables;

			if (retTables.IsNullOrEmpty())
			{
				LogRetrievingViewsError(workspceId, artifactTypeId);
				throw new Exception($"No result returned when call to {nameof(ISearchManager.RetrieveViewsByContextArtifactID)} method!");
			}
			return ConvertToView(retTables);
		}

		private List<ViewDTO> ConvertToView(DataTableCollection retTables)
		{
			return retTables[0]
				.AsEnumerable()
				.Select(item => new ViewDTO
				{
					ArtifactId = item.Field<int>("ArtifactID"),
					Name = item.Field<string>("Name"),
					IsAvailableInObjectTab = item.Field<bool>("AvailableInObjectTab"),
					Order = item.Field<int?>("Order")
				})
				.Where(view => view.IsAvailableInObjectTab)
				.OrderBy(view => view.Order)
				.ToList();
		}

		#endregion //Methods

		#region Logging

		private void LogRetrievingViewsError(int workspceId, int artifactTypeId)
		{
			_logger.LogError("No views returned for ArtifactType {ArtifactTypeId} in Workspace {WorkspaceId}.", artifactTypeId, workspceId);
		}

		#endregion
	}
}