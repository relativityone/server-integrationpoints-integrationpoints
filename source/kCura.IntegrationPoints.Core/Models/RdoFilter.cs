using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Models
{
	public class RdoFilter
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
					"History","Event Handler","Install Event Handler","Source Provider","Integration Point"
				};
			}
		}

		public IEnumerable<ObjectType> FilterRdo()
		{
			var list = _rdoQuery.GetAllTypes(_serviceContext.WorkspaceUserID);
			return list.Where(ot => !systemRdo.Contains(ot.Name));
		}
	}
}
