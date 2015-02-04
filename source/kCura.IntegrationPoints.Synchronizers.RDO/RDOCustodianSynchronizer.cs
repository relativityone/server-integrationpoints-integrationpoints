using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RDOCustodianSynchronizer : RdoSynchronizer
	{
		public RDOCustodianSynchronizer(RelativityFieldQuery fieldQuery, RelativityRdoQuery rdoQuery)
			: base(fieldQuery, rdoQuery)
		{
		}
	}
}
