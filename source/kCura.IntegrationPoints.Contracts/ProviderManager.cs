using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts
{

	/// <summary>
	/// Entry point into the App domain to create the Provider
	/// </summary>
	public class DomainManager : MarshalByRefObject
	{
		public Provider.IDataSourceProvider GetProvider(Guid identifer)
		{
			return ProviderBuilder.Current.GetFactory().CreateProvider(identifer);
		}

	}
}
