using Relativity.IntegrationPoints.Tests.Functional;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Common.Extensions
{
	public static class WorkspaceExtensions
	{
		public static void InstallApplication(this Workspace workspace, string applicationName)
		{
			ILibraryApplicationService applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();
			applicationService.InstallToWorkspace(workspace.ArtifactID, applicationService.Get(applicationName).ArtifactID);
		}

		public static void InstallLegalHold(this Workspace workspace)
		{
			InstallApplication(workspace, Const.Application.LEGAL_HOLD_APPLICATION_NAME);
		}
	}
}
