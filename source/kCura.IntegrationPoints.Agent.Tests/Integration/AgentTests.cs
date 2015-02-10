using System;
using kCura.Relativity.Client;
using NUnit.Framework;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	public class AgentTests
	{
		[Test]
		[Explicit]
		public void CreateJob()
		{
			//work in progress
			var client = new RSAPIClient(new Uri("net.pipe://localhost/Relativity.Services"), new IntegratedAuthCredentials())
			{
				APIOptions = { WorkspaceID = 1018513 }
			};

			IntegrationPoint ip = new IntegrationPoint();
			//ip.SourceProvider=
		}
	}
}
