using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public class RdoGetter
	{
		public RelativityRdoQuery _RdoQuery;

		public RdoGetter(RelativityRdoQuery rdoQuery)
		{
			_RdoQuery = rdoQuery; 
		}
		public List<ObjectType> getAllRdo()
		{
			return _RdoQuery.GetAllRdo();
		}
	}
}
