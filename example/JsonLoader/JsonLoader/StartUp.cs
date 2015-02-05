using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts;

namespace JsonLoader
{
	public class StartUp : kCura.IntegrationPoints.Contracts.IStartUp
	{
		public void Execute()
		{
			PluginBuilder.Current.SetProviderFactory(new DIProviderFactory());
		}
	}
}
