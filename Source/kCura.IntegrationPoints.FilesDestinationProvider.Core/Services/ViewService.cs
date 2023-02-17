using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
    public class ViewService : IViewService
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IAPILog _logger;

        public ViewService(IHelper helper)
        {
            _servicesMgr = helper.GetServicesManager();
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ViewService>();
        }

        public List<ViewDTO> GetViewsByWorkspaceAndArtifactType(int workspceId, int artifactTypeId)
        {
            using (ISearchService searchService = _servicesMgr.CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser))
            {
                List<ViewDTO> views = new List<ViewDTO>();

                DataSet dataSet = searchService.RetrieveViewsByContextArtifactIDAsync(workspceId, artifactTypeId, false, string.Empty).GetAwaiter().GetResult()?.Unwrap();
                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    views = ConvertToView(dataSet.Tables);
                }
                else
                {
                    LogRetrievingViewsError(workspceId, artifactTypeId);
                    throw new Exception($"No result returned when call to {nameof(ISearchService.RetrieveViewsByContextArtifactIDAsync)} method!");
                }

                return views;
            }
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

        private void LogRetrievingViewsError(int workspceId, int artifactTypeId)
        {
            _logger.LogError("No views returned for ArtifactType {ArtifactTypeId} in Workspace {WorkspaceId}.", artifactTypeId, workspceId);
        }
    }
}
