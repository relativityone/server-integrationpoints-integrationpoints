using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Controllers;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	[TestFixture]
	public class BaseControllerTests
	{
		public class MockController : BaseController
		{
			public void LogError(Exception e, string controller, string action)
			{
				base.LogException(e, controller, action);
			}
		}

		[Test]
		public void LogException_CallsCreateErrorWithAction()
		{
			//ARRANGE
			var controller = new MockController();
			controller.SessionService = NSubstitute.Substitute.For<ISessionService>();

			controller.CreateError = NSubstitute.Substitute.For<ErrorService>(NSubstitute.Substitute.For<Data.Queries.CreateErrorRdo>(NSubstitute.Substitute.For<IRSAPIClient>()));
			//ACT
			controller.LogError(new Exception(), "controller", "action");
			
			//ASSERT
			controller.CreateError.Received().Log(Arg.Is<ErrorModel>(x=>x.Message == "controller/action"));
		}

	}
}
