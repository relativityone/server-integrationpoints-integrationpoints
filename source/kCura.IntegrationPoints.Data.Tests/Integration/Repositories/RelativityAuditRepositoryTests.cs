using System.Security.Claims;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[Category("Integration Tests")]
	public class RelativityAuditRepositoryTests
	{
		private RelativityAuditRepository _instance;

		[SetUp]
		public void SetUp()
		{
			int workspaceId = 1536135;
			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				ClaimsPrincipalFactory factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(9);
			};
			var serviceContext = ClaimsPrincipal.Current.GetUnversionContext(workspaceId);
			_instance = new RelativityAuditRepository(serviceContext);
		}

		[Test]
		public void CreateAuditRecord()
		{
			AuditElement detail = new AuditElement()
			{
				AuditMessage = "Test audit."
			};
			_instance.CreateAuditRecord(1038143, detail);
		}
	}
}