using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.API.Foundation;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class AuditRepositoryTests
	{
		private IAuditService _auditService;

		[SetUp]
		public void SetUp()
		{
			_auditService = Substitute.For<IAuditService>();
		}

		[Test]
		public void AuditExportTest(
			[Values(true, false)] bool expectedResult)
		{
			_auditService.CreateAuditForExport(Arg.Any<ExportStatistics>()).Returns(expectedResult);

			var auditRepository = new AuditRepository(_auditService);
			var exportStats = new ExportStatistics();
			bool result = auditRepository.AuditExport(exportStats);

			_auditService.Received(1).CreateAuditForExport(exportStats);
			Assert.AreEqual(expectedResult, result);
		}
	}
}
