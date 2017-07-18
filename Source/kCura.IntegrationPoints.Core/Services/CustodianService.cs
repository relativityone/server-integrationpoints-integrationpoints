using System;
using System.Linq;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class CustodianService
	{
		private readonly IRsapiRdoQuery _rdoQuery;
		public CustodianService(IRsapiRdoQuery rdoQuery)
		{
			_rdoQuery = rdoQuery;
		}

		public bool IsCustodian(int id)
		{
			var guids = _rdoQuery.GetObjectType(id).Guids;
			return guids.Any(x => x.Equals(Guid.Parse(GlobalConst.Custodian)));
		}
	}
}
