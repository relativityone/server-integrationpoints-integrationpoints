using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using NUnit.Framework;

namespace Rip.SystemTests.IntegrationPointServices
{
	[TestFixture]
	public class IntegrationPointServiceTests
	{
		private IWindsorContainer _container => SystemTestsFixture.Container;
		private IIntegrationPointService _integrationPointService;

		[OneTimeSetUp]
		public void OneSetup()
		{
			_integrationPointService = _container.Resolve<IIntegrationPointService>();
		}

		[Test]
		public void Test1()
		{

		}
	}
}
