using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.LDAPProvider
{
	public interface ILDAPDataFormatter
	{
		object FormatData(object initialData);
	}
}
