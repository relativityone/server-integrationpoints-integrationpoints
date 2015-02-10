using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Provider;

namespace JsonLoader
{
	public class DIProviderFactory : kCura.IntegrationPoints.Contracts.DefaultProviderFactory
	{
		//this is just here to show that you can do dependency injection with this library if you so choose.
		//this default ProviderFactory just uses Activator.Create instance
		protected override IDataSourceProvider CreateInstance(Type providerType)
		{
			return new JsonProvider(new JsonHelper());
		}
	}
}
