using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.SourceProviderInstaller;

namespace Provider.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Update My First provider - On Every Install")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("bd0c749d-dc8d-4b9c-b4f7-2ea4f508089e")]
	public class RegisterNewIntegrationPoint : IntegrationPointSourceProviderInstaller
	{
		public override IDictionary<Guid, SourceProvider> GetSourceProviders()
		{
			Dictionary<Guid, SourceProvider> sourceProviders = new Dictionary<Guid, SourceProvider>();
			var myFirstProviderEntry = new SourceProvider();
			myFirstProviderEntry.Name = "My First Provider";
			myFirstProviderEntry.Url = string.Format("/%applicationpath%/CustomPages/{0}/Provider/Settings", GlobalConstants.APPLICATION_GUID);
			sourceProviders.Add(Guid.Parse(GlobalConstants.FIRST_PROVIDER_GUID), myFirstProviderEntry);

			return sourceProviders;
		}
	}
}
