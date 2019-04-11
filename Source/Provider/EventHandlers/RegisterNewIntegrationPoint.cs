using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Provider.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Update My First provider - On Every Install")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("bd0c749d-dc8d-4b9c-b4f7-2ea4f508089e")]
	public class RegisterNewIntegrationPoint : kCura.IntegrationPoints.SourceProviderInstaller.IntegrationPointSourceProviderInstaller
	{
		public override IDictionary<Guid, kCura.IntegrationPoints.Contracts.SourceProvider> GetSourceProviders()
		{
			var sourceProviders = new Dictionary<Guid, kCura.IntegrationPoints.Contracts.SourceProvider>();
			var myFirstProviderEntry = new kCura.IntegrationPoints.Contracts.SourceProvider
			{
				Name = "My First Provider",
				Url = string.Format("/%applicationpath%/CustomPages/{0}/Provider/Settings", GlobalConstants.APPLICATION_GUID),
				ViewDataUrl = string.Format("/%applicationpath%/CustomPages/{0}/%appId%/api/ProviderAPI/GetViewFields", GlobalConstants.APPLICATION_GUID)
			};

			sourceProviders.Add(Guid.Parse(GlobalConstants.FIRST_PROVIDER_GUID), myFirstProviderEntry);
			return sourceProviders;
		}
	}
}
