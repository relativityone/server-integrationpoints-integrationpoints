using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Helpers
{
	public class TestBase
	{
		private IWindsorContainer _windsorContainer;

		[SetUp]
		public void TestFixtureSetUp()
		{
			_windsorContainer = new WindsorContainer();	
		}

		[TearDown]
		public void TearDown()	
		{
			_windsorContainer.Dispose();	
		}

		public T GetMock<T>() where T:class
		{
			T mock = NSubstitute.Substitute.For<T>();
			_windsorContainer.Register(Component.For<T>().Instance(mock).LifestyleTransient());

			return mock;
		}

		public T ResolveInstance<T>() where T : class
		{
			_windsorContainer.Register(Component.For<T>().LifestyleTransient());
			T resolvedClass = _windsorContainer.Resolve<T>();

			return resolvedClass;
		}
	}
}
