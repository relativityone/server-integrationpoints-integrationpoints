using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using Relativity.IntegrationPoints.Contracts;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [kCura.EventHandler.CustomAttributes.Description("Register the RIP Import Provider")]
    [kCura.EventHandler.CustomAttributes.RunOnce(true)]
    [Guid("01E26CBC-98CA-48A6-942E-FD546E2D5F7E")]
    public class RegisterImportProvider : InternalSourceProviderInstaller
	{
        public RegisterImportProvider(IToggleProvider toggleProvider) : base(toggleProvider)
        {
            
        }
        public override IDictionary<Guid, SourceProvider> GetSourceProviders()
        {
            return new Dictionary<Guid, SourceProvider>()
            {
                {
                    new Guid("548F0873-8E5E-4DA6-9F27-5F9CDA764636"),
                    new SourceProvider()
                    {
                        Name = "Load File",
                        Url = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/ImportProvider/ImportSettings",
                        ViewDataUrl = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/%appId%/api/ImportProviderDocument/ViewData"
                    }
                }
            };
        }
    }
}
