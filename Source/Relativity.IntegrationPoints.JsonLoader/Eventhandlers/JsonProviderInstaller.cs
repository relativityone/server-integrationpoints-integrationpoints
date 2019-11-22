using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.SourceProviderInstaller;

namespace Relativity.IntegrationPoints.JsonLoader.Eventhandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Registers the JSON provider")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)] // Executes every install
	[Guid("64110733-03F8-4DAC-958D-31E9DFDA6071")]
	public class JsonProviderInstaller : IntegrationPointSourceProviderInstaller
	{
		public override IDictionary<Guid, SourceProvider> GetSourceProviders()
		{
			return new Dictionary<Guid, SourceProvider>()
			{
				{
					new Guid(Constants.JSON_SOURCE_PROVIDER_GUID),
					new SourceProvider()
					{
						Name = "JSON",
						Url = $"/%applicationpath%/CustomPages/{Constants.APPLICATION_GUID}/Home/",
						ViewDataUrl = $"/%applicationpath%/CustomPages/{Constants.APPLICATION_GUID}/api/view/",
						Configuration = new SourceProviderConfiguration()
						{
							AlwaysImportNativeFiles = false,
							AlwaysImportNativeFileNames = false,
							OnlyMapIdentifierToIdentifier = false,
							CompatibleRdoTypes = new List<Guid>()
							{
								new Guid(Constants.DOCUMENT_RDO_GUID),
								new Guid(Constants.SAMPLE_JSON_OBJECT_GUID)
							}
						}
					}
				}
			};
		}
	}
}
