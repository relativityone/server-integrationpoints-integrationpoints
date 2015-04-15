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
						Name = "JSON",
						Url = "/%applicationpath%/CustomPages/f0fd184d-d2ca-4be9-83ec-884955eea8ff/Home/",
						ViewDataUrl = "/%applicationpath%/CustomPages/f0fd184d-d2ca-4be9-83ec-884955eea8ff/api/view/"
					}
				}
			};
		}
	}
}
