using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class HelperBase
	{
		protected static Helper Helper { get; set; }

		public HelperBase(Helper helper)
		{
			Helper = helper;
		}
	}
}
