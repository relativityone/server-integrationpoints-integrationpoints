using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.SourceProviderInstaller;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	[TestFixture]
	public class InstallDllTests
	{
		[Test]
		[Explicit]
		public void Test()
		{
			Debug.WriteLine("");
			ImportHelper.ExtractEmbeddedResource(@"C:\SourceCode\LDAPSync", "kCura.IntegrationPoints.SourceProviderInstaller.Resources", "kCura.IntegrationPoints.LDAPProvider.dll");
			Debug.WriteLine("");
		}
	}
}
