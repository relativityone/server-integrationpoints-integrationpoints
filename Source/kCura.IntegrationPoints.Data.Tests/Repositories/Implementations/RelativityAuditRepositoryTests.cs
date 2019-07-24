using System;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API.Foundation;
using IAuditRepository = Relativity.API.Foundation.Repositories.IAuditRepository;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class RelativityAuditRepositoryTests
	{
		private IRelativityAuditRepository _sut;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
		private Mock<IExternalServiceSimpleInstrumentation> _instrumentationMock;
		private Mock<IAuditRepository> _foundationAuditRepositoryMock;

		[SetUp]
		public void SetUp()
		{
			_instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
			_instrumentationMock = new Mock<IExternalServiceSimpleInstrumentation>();
			_instrumentationMock.Setup(x => x.Execute(It.IsAny<Func<object>>())).Returns<Func<object>>(x => x.Invoke());
			_instrumentationProviderMock
				.Setup(x => x.CreateSimple(
					ExternalServiceTypes.API_FOUNDATION, 
					nameof(IAuditRepository),
					nameof(IAuditRepository.CreateAuditRecord)))
				.Returns(_instrumentationMock.Object);
			_foundationAuditRepositoryMock = new Mock<IAuditRepository>();
			_sut = new RelativityAuditRepository(_foundationAuditRepositoryMock.Object, _instrumentationProviderMock.Object);
		}

		[Test]
		public void CreateAuditRecord_ShouldCallFoundationRepositoryWithInstrumentation()
		{
			// arrange
			const int artifactID = 1000123;
			var auditElement = new AuditElement {AuditMessage = "test"};
			const string auditElementXmlString = "<auditElement>\r\n  <auditMessage>test</auditMessage>\r\n</auditElement>";

			// act
			_sut.CreateAuditRecord(artifactID, auditElement);

			// assert
			_instrumentationProviderMock.Verify(
				x => x.CreateSimple(
					ExternalServiceTypes.API_FOUNDATION, 
					nameof(IAuditRepository),
					nameof(IAuditRepository.CreateAuditRecord)), 
				Times.Once);
			_instrumentationMock.Verify(x => x.Execute(It.IsAny<Func<object>>()), Times.Once);
			_foundationAuditRepositoryMock.Verify(x => x.CreateAuditRecord(It.Is<IAuditRecord>(auditRecord =>
					auditRecord.ArtifactID == artifactID &&
					auditRecord.Action == AuditAction.Run &&
					auditRecord.Details.ToString() == auditElementXmlString &&
					auditRecord.ExecutionTime.Equals(TimeSpan.Zero))),
				Times.Once);
		}
	}
}
