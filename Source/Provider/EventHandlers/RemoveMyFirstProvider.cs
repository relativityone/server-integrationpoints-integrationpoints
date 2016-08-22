﻿using System.Runtime.InteropServices;

namespace Provider.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Update My First provider - Uninstall")]
	[kCura.EventHandler.CustomAttributes.RunOnce(false)]
	[Guid("a2e8a386-af25-42f9-ab99-1063e8547114")]
	public class RemoveMyFirstProvider : kCura.IntegrationPoints.SourceProviderInstaller.IntegrationPointSourceProviderUninstaller
	{
	}
}
