using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public class RdoGetter
	{
		public IRsapiRdoQuery _RdoQuery;

		public RdoGetter(IRsapiRdoQuery rdoQuery)
		{
			_RdoQuery = rdoQuery; 
		}
		public List<ObjectType> GetAllRdo()
		{
			return _RdoQuery.GetAllRdo();
		}
	}
}
