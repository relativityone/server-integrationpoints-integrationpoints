using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [kCura.EventHandler.CustomAttributes.Description("Update Integration Points Entities - On Every Install")]
    [kCura.EventHandler.CustomAttributes.RunOnce(true)]
    [Guid("02ec5d64-208a-44fb-a5e3-c3a1103e7da7")]
    public class RegisterLDAPInstaller : InternalSourceProviderInstaller
    {
        public override IDictionary<Guid, SourceProvider> GetSourceProviders()
        {
            return new Dictionary<Guid, SourceProvider>
            {
                {
                    new Guid("5bf1f2c2-9670-4d6e-a3e9-dbc83db6c232"),
                    new SourceProvider()
                    {
                        Name = "LDAP",
                        Url = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/IntegrationPoints/LDAPConfiguration/",
                        ViewDataUrl = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/%appId%/api/ldap/view"
                    }
                }
            };
        }
    }
}