using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.DocumentTransferProvider.Shared;
using kCura.IntegrationPoints.SourceProviderInstaller;
using SourceProvider = kCura.IntegrationPoints.SourceProviderInstaller.SourceProvider;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[kCura.EventHandler.CustomAttributes.Description("Add Relativity provider into relativity integration point")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("93057ef0-9b7e-4fc5-9691-7f97e98cc703")]
	public class RegisterDocumentTransferProvider : IntegrationPointSourceProviderInstaller
	{
		/// <summary>
		/// Define guid with source provider to allow intergration core to be used for source provider installation.
		/// </summary>
		/// <returns>a Dictionary with mapped Guid and Relativity source provider</returns>
		public override IDictionary<Guid, SourceProvider> GetSourceProviders()
		{
			return new Dictionary<Guid, SourceProvider>()
			{
				{
					new Guid(Constants.RELATIVITY_PROVIDER_GUID),
					new SourceProvider()
					{
						Name = Constants.RELATIVITY_PROVIDER_NAME,
						Url = String.Format("/%applicationpath%/CustomPages/{0}/IntegrationPoints/{1}/",  Constants.RELATIVITY_CUSTOMPAGE_GUID, Constants.RELATIVITY_PROVIDER_CONFIGURATION),
						ViewDataUrl = String.Format("/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/%appId%/api/relativity/view"),
						Configuration = new SourceProviderConfiguration()
						{
							CompatibleRdoTypes = new List<Guid>()
							{
								// doc rdo
								new Guid("15C36703-74EA-4FF8-9DFB-AD30ECE7530D")
							}
						}
					}
				}
			};
		}
	}
}