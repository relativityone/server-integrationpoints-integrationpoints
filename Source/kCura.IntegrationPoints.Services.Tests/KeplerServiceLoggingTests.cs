using System;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests
{
	public class KeplerServiceLoggingTests : TestBase
	{
		private KeplerServiceToTestExecute _keplerService;
		private ILog _logger;
		private IWindsorContainer _container;

		public override void SetUp()
		{
			var permissionRepository = Substitute.For<IPermissionRepository>();
			permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
			permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View).Returns(true);

			var permissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
			permissionRepositoryFactory.Create(Arg.Any<IHelper>(), Arg.Any<int>()).Returns(permissionRepository);

			_container = Substitute.For<IWindsorContainer>();
			_logger = Substitute.For<ILog>();
			_keplerService = new KeplerServiceToTestExecute(_logger, permissionRepositoryFactory, _container);
		}

		[Test]
		public void ItShouldLogErrorDuringTaskExecution()
		{
			string expectedMessage = "exception_message_915";
			var expectedException = new Exception(expectedMessage);

			_container.Resolve<int>().Returns(1);

			try
			{
				_keplerService.Execute<char, int>(a => { throw expectedException; }, 1).Wait();
			}
			catch
			{
				//Ignore as throwing exception is tested in different test
			}

			_logger.Received(1).LogError(expectedException, "{}", typeof(int));
		}

		[Test]
		public void ItShouldLogErrorBeforeTastExecution()
		{
			string expectedMessage = "exception_message_729";
			var expectedException = new Exception(expectedMessage);

			_container.Resolve<double>().Throws(expectedException);

			try
			{
				_keplerService.Execute<float, double>(a => 5, 1).Wait();
			}
			catch
			{
				//Ignore as throwing exception is tested in different test
			}

			_logger.Received(1).LogError(expectedException, "{}", typeof(double));
		}

		[Test]
		public void ItShouldHideExceptionThrownFromTask()
		{
			Assert.That(() => _keplerService.Execute<int, int>(a => { throw new Exception(); }, 1).Wait(),
				Throws.InnerException.TypeOf<InternalServerErrorException>()
				.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}

		[Test]
		public void ItShouldHideExceptionThrownBeforeTask()
		{
			_container.Resolve<int>().Throws(new Exception());

			Assert.That(() => _keplerService.Execute<int, int>(a => 6, 1).Wait(),
				Throws.Exception.TypeOf<InternalServerErrorException>()
				.With.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));
		}
	}

	internal class KeplerServiceToTestExecute : KeplerServiceBase
	{
		private readonly IWindsorContainer _container;

		public KeplerServiceToTestExecute(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory)
		{
			_container = container;
		}

		public KeplerServiceToTestExecute(ILog logger, IWindsorContainer container) : base(logger)
		{
			_container = container;
		}

		public new Task<T> Execute<T, TT>(Func<TT, T> a, int workspaceId)
		{
			return base.Execute(a, workspaceId);
		}

		protected override Installer Installer { get; }

		protected override IWindsorContainer GetDependenciesContainer(int workspaceArtifactId)
		{
			return _container;
		}
	}
}