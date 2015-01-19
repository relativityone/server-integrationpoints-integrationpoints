using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using kCura.IntegrationPoints.SourceProviderInstaller;

namespace JsonLoader.Installer
{
	[kCura.EventHandler.CustomAttributes.Description("Update Json provider - On Every Install")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("64110733-03F8-4DAC-958D-31E9DFDA6071")]
	public class RunEveryTimeInstaller : IntegrationPointSourceProviderInstaller
	{
		public override IDictionary<System.Guid, SourceProvider> GetSourceProviders()
		{
			return new Dictionary<Guid, SourceProvider>()
			{
				{
					new Guid(GlobalConst.JSON_SOURCE_PROVIDER_GUID),
					new SourceProvider()
					{
						Name = "Json File Source Provider.",
						Url = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/IntegrationPoints/LDAPConfiguration/"
					}
				}
			};
		}
	}
}
