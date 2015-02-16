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
		public RSAPIRdoQuery _RdoQuery;

		public RdoGetter(RSAPIRdoQuery rdoQuery)
		{
			_RdoQuery = rdoQuery; 
		}
		public List<ObjectType> GetAllRdo()
		{
			return _RdoQuery.GetAllRdo();
		}
	}
}
