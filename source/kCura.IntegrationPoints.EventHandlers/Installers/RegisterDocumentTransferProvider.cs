namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using kCura.IntegrationPoints.DocumentTransferProvider.Shared;
	using kCura.IntegrationPoints.SourceProviderInstaller;

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
					new Guid(Constants.PROVIDER_GUID),
					new SourceProvider()
					{
						Name = Constants.PROVIDER_NAME,
						Url = String.Format("/%applicationpath%/CustomPages/{0}/IntegrationPoints/{1}/",  Constants.CUSTOMPAGE_GUID, Constants.PROVIDER_CONFIGURATION),
					}
				}
			};
		}
	}
}