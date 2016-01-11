namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using kCura.IntegrationPoints.SourceProviderInstaller;
	using kCura.IntegrationPoints.DocumentTransferProvider.Shared;

	[kCura.EventHandler.CustomAttributes.Description("Add document transfer provider into relativity integration point")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("93057ef0-9b7e-4fc5-9691-7f97e98cc703")]
	public class RegisterDocumentTransferProvider : IntegrationPointSourceProviderInstaller
	{
		public override IDictionary<Guid, SourceProvider> GetSourceProviders()
		{
			return new Dictionary<Guid, SourceProvider>()
			{
				{
					new Guid(Constants.PROVIDER_GUID),
					new SourceProvider()
					{
						Name = Constants.PROVIDER_NAME,
						Url = String.Format("/%applicationpath%/CustomPages/{0}/IntegrationPoints/{1}/", Constants.PROVIDER_NAME, Constants.PROVIDER_CONFIGURATION),
						ViewDataUrl = String.Format("/%applicationpath%/CustomPages/{0}/%appId%/api/{1}/view", Constants.PROVIDER_NAME, Constants.PROVIDER_VIEW)
					}
				}
			};
		}
	}
}