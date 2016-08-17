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
        public override IDictionary<Guid, kCura.IntegrationPoints.SourceProviderInstaller.SourceProvider> GetSourceProviders()
        {
            Dictionary<Guid, kCura.IntegrationPoints.SourceProviderInstaller.SourceProvider> sourceProviders = new Dictionary<Guid, kCura.IntegrationPoints.SourceProviderInstaller.SourceProvider>();
            var myFirstProviderEntry = new kCura.IntegrationPoints.SourceProviderInstaller.SourceProvider();
            myFirstProviderEntry.Name = "My First Provider";
            myFirstProviderEntry.Url = string.Format("/%applicationpath%/CustomPages/{0}/Provider/Settings", GlobalConstants.APPLICATION_GUID);
            myFirstProviderEntry.ViewDataUrl = string.Format("/%applicationpath%/CustomPages/{0}/%appId%/api/ProviderAPI/GetViewFields", GlobalConstants.APPLICATION_GUID);
            sourceProviders.Add(Guid.Parse(GlobalConstants.FIRST_PROVIDER_GUID), myFirstProviderEntry);

            return sourceProviders;
        }
    }
}
