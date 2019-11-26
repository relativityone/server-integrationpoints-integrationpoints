using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.SourceProviderInstaller;

namespace Relativity.IntegrationPoints.MyFirstProvider.Provider.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Update My First provider - On Every Install")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("bd0c749d-dc8d-4b9c-b4f7-2ea4f508089e")]
	public class RegisterNewIntegrationPoint : IntegrationPointSourceProviderInstaller
	{
		public override IDictionary<Guid, SourceProvider> GetSourceProviders()
		{
			var sourceProviders = new Dictionary<Guid, SourceProvider>();
			var myFirstProviderEntry = new SourceProvider
			{
				Name = "My First Provider",
				Url = $"/%applicationpath%/CustomPages/{GlobalConstants.APPLICATION_GUID}/Provider/Settings",
				ViewDataUrl = $"/%applicationpath%/CustomPages/{GlobalConstants.APPLICATION_GUID}/%appId%/api/ProviderAPI/GetViewFields"
			};

			sourceProviders.Add(Guid.Parse(GlobalConstants.FIRST_PROVIDER_GUID), myFirstProviderEntry);
			return sourceProviders;
		}
	}
}
