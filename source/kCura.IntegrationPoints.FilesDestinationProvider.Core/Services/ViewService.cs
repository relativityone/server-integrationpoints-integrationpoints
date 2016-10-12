
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS.Service.Export;
using ViewDTO = kCura.IntegrationPoints.Domain.Models.ViewDTO;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ViewService : IViewService
	{
		#region Fields

		private readonly IServiceManagerProvider _serviceManagerProvider;

		#endregion //Fields

		#region Constructors

		public ViewService(IServiceManagerProvider serviceManagerProvider)
		{
			_serviceManagerProvider = serviceManagerProvider;
		}

		#endregion //Constructors

		#region Methods

		public List<ViewDTO> GetViewsByWorkspaceAndArtifactType(int workspceId, int artifactTypeId) 
		{
			ISearchManager searchManager = _serviceManagerProvider.Create<ISearchManager, SearchManagerFactory>();
			// Third argument has to be always False in case of RIP Export
			DataTableCollection retTables = searchManager.RetrieveViewsByContextArtifactID(workspceId, artifactTypeId, false).Tables;

			if (retTables.IsNullOrEmpty())
			{
				throw new Exception($"No result returned when call to {nameof(ISearchManager.RetrieveViewsByContextArtifactID)} method!");
			}
			return ConvertToView(retTables);
		}

		private List<ViewDTO> ConvertToView(DataTableCollection retTables)
		{
			return retTables[0]
				.AsEnumerable()
				.Select(item => new ViewDTO()
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


	}
}
