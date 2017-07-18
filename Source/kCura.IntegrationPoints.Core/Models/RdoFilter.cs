using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Models
{
	public class RdoFilter : IRdoFilter
	{
		private IObjectTypeQuery _rdoQuery;
		private readonly ICaseServiceContext _serviceContext;
		public RdoFilter(IObjectTypeQuery rdoQuery, ICaseServiceContext serviceContext)
		{
			_rdoQuery = rdoQuery;
			_serviceContext = serviceContext;
		}

		private List<string> systemRdo
		{
			get
			{
				return new List<string>
				{
					"History","Event Handler","Install Event Handler","Source Provider","Integration Point", "Relativity Source Case", "Destination Workspace", "Relativity Source Job"
                };
			}
		}

		public IEnumerable<ObjectType> GetAllViewableRdos()
		{
			var list = _rdoQuery.GetAllTypes(_serviceContext.WorkspaceUserID);
			return list.Where(ot => !systemRdo.Contains(ot.Name));
		}
	}
}
