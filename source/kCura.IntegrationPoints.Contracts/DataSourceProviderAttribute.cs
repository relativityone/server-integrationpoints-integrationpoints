using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts
{
	public class DataSourceProviderAttribute : Attribute
	{
		public Guid Identifier { get; private set; }

		public DataSourceProviderAttribute(string identifier)
		{
			this.Identifier = Guid.Parse(identifier);
		}
	}
}
