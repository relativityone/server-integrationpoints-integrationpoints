namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using kCura.IntegrationPoints.SourceProviderInstaller;

	[kCura.EventHandler.CustomAttributes.Description("Add document transfer provider into relativity integration point")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("93057ef0-9b7e-4fc5-9691-7f97e98cc703")]
	public class RegisterDocumentTransferProvider : IntegrationPointSourceProviderInstaller
	{
		const String guid = "423b4d43-eae9-4e14-b767-17d629de4bb2";
		const String name = "Document Transfer";

		public RegisterDocumentTransferProvider()
		{
		}

		public override IDictionary<Guid, SourceProvider> GetSourceProviders()
		{
			return new Dictionary<Guid, SourceProvider>()
			{
				{
					new Guid(guid),
					new SourceProvider()
					{
						Name = name,
						Url = String.Format("/%applicationpath%/CustomPages/{0}/IntegrationPoints/LDAPConfiguration/", guid),
						ViewDataUrl = String.Format("/%applicationpath%/CustomPages/{0}/%appId%/api/ldap/view", guid)
					}
				}
			};
		}
	}
}