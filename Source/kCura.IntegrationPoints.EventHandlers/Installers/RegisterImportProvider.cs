using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.SourceProviderInstaller;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    public class RegisterImportProvider : IntegrationPointSourceProviderInstaller
    {
        public override IDictionary<Guid, SourceProvider> GetSourceProviders()
        {
            return new Dictionary<Guid, SourceProvider>()
            {
                {
                    new Guid("548F0873-8E5E-4DA6-9F27-5F9CDA764636"),
                    new SourceProvider()
                    {
                        Name = "Relativity Import",
                        Url = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/ImportProvider/GetDefaultFtpSettings"
                        //TODO: add api controller to view the source configuration data
                        //ViewDataUrl = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/%appId%/api/FtpProviderAPI/view"
                    }
                }
            };
        }
    }
}
