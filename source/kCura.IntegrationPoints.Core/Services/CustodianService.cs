using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public class CustodianService
	{
		private readonly RelativityRdoQuery _rdoQuery;
		public CustodianService(RelativityRdoQuery rdoQuery)
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
