using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Models.RDO
{
	public class IntegrationPoint
	{
		private Data.IntegrationPoints _rdo;
		internal IntegrationPoint(Data.IntegrationPoints rdo)
		{
			_rdo = rdo;
		}
		
	}
}
