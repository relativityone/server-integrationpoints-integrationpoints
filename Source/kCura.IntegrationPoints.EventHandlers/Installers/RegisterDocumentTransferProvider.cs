using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[kCura.EventHandler.CustomAttributes.Description("Add Relativity provider into relativity integration point")]
	[kCura.EventHandler.CustomAttributes.RunOnce(true)]
	[Guid("93057ef0-9b7e-4fc5-9691-7f97e98cc703")]
	public class RegisterDocumentTransferProvider : InternalSourceProviderInstaller
    {
		/// <summary>
		/// Define guid with source provider to allow integration core to be used for source provider installation.
		/// </summary>
		/// <returns>a Dictionary with mapped Guid and Relativity source provider</returns>
		public override IDictionary<Guid, SourceProvider> GetSourceProviders()
		{
			return new Dictionary<Guid, SourceProvider>()
			{
				{
					new Guid(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID),
					new SourceProvider()
					{
						Name = Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME,
						Url = $"/%applicationpath%/CustomPages/{Core.Constants.IntegrationPoints.RELATIVITY_CUSTOMPAGE_GUID}/IntegrationPoints/{Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_CONFIGURATION}/",
						ViewDataUrl = "/%applicationpath%/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/%appId%/api/relativity/view",
						Configuration = new SourceProviderConfiguration()
						{
							AlwaysImportNativeFiles = true,
							AlwaysImportNativeFileNames = true,
							CompatibleRdoTypes = new List<Guid>
							{
								Core.Constants.ObjectTypeArtifactTypesGuid.Document
							},
							AvailableImportSettings = new ImportSettingVisibility
							{
								AllowUserToMapNativeFileField = false
							},
							OnlyMapIdentifierToIdentifier = true
						}
					}
				}
			};
		}
	}
}