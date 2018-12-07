using System;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.API.Foundation;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class AuditRepositoryTests
	{
		private IAuditService _auditService;
		private IExternalServiceInstrumentationProvider _instrumentationProvider;
		private IExternalServiceSimpleInstrumentation _instrumentation;

		[SetUp]
		public void SetUp()
		{
			_auditService = Substitute.For<IAuditService>();
			_instrumentation = Substitute.For<IExternalServiceSimpleInstrumentation>();
			_instrumentation.Execute(Arg.Any<Func<bool>>())
				.Returns(c => c.ArgAt<Func<bool>>(0).Invoke());
			_instrumentationProvider = Substitute.For<IExternalServiceInstrumentationProvider>();
			_instrumentationProvider.CreateSimple(
					Arg.Any<string>(),
					Arg.Any<string>(),
					Arg.Any<string>())
				.Returns(_instrumentation);
		}

		[Test]
		public void ShouldReturnProperResultWhenCallAuditExportAndInstrumentSuccessfully([Values(true, false)] bool expectedResult)
		{
			//arrange
			_auditService.CreateAuditForExport(Arg.Any<ExportStatistics>()).Returns(expectedResult);
			var auditRepository = new AuditRepository(_auditService, _instrumentationProvider);
			var exportStats = new ExportStatistics();

			//act
			bool result = auditRepository.AuditExport(exportStats);

			//assert
			_auditService.Received().CreateAuditForExport(exportStats);
			_instrumentationProvider.Received().CreateSimple(
				"API.Foundation",
				"IAuditService",
				"CreateAuditForExport");
			_instrumentation.Received().Execute(Arg.Any<Func<bool>>());
			expectedResult.Should().Be(result);
		}

		[Test]
		public void ShouldInstrumentSuccessfullyWhenIAuditServiceFails()
		{
			//arrange
			_auditService.CreateAuditForExport(Arg.Any<ExportStatistics>()).Throws<Exception>();
			var auditRepository = new AuditRepository(_auditService, _instrumentationProvider);
			var exportStats = new ExportStatistics();

			//act
			Action action = () => auditRepository.AuditExport(exportStats);

			//assert
			action.ShouldThrow<Exception>();
			_auditService.Received().CreateAuditForExport(exportStats);
			_instrumentationProvider.Received().CreateSimple(
				"API.Foundation",
				"IAuditService",
				"CreateAuditForExport");
			_instrumentation.Received().Execute(Arg.Any<Func<bool>>());
		}

		[Test]
		public void ShouldReturnFalseWhenCallAuditExportWithNullAndInstrumentSuccessfully()
		{
			//arrange
			_auditService.CreateAuditForExport(Arg.Any<ExportStatistics>()).Returns(false);
			var auditRepository = new AuditRepository(_auditService, _instrumentationProvider);

			//act
			bool result = auditRepository.AuditExport(null);

			//assert
			_auditService.Received().CreateAuditForExport(null);
			_instrumentationProvider.Received().CreateSimple(
				"API.Foundation",
				"IAuditService",
				"CreateAuditForExport");
			_instrumentation.Received().Execute(Arg.Any<Func<bool>>());
			result.Should().BeFalse();
		}
	}
}
