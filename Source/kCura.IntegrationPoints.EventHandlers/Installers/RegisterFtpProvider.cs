using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [kCura.EventHandler.CustomAttributes.Description("Register the RIP FTP Provider")]
    [kCura.EventHandler.CustomAttributes.RunOnce(true)]
    [Guid("D1578A7B-E662-4DEE-BD59-C224E27F5296")]
    public class RegisterFtpProvider : InternalSourceProviderInstaller
	{
        public override IDictionary<Guid, SourceProvider> GetSourceProviders()
        {
            return new Dictionary<Guid, SourceProvider>()
            {
                {
                    new Guid("85120BC8-B2B9-4F05-99E9-DE37BB6C0E15"),
                    new SourceProvider()
                    {
                        Name = "FTP (CSV File)",
                        Url = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/FtpProvider/GetDefaultFtpSettings",
                        ViewDataUrl = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/%appId%/api/FtpProviderAPI/view"
                    }
                }
            };
        }
    }
}