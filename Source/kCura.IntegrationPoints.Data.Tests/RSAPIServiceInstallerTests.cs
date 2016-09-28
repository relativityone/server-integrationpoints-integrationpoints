using System;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Installers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests
{
	[TestFixture]
	public class RSAPIServiceInstallerTests
	{
		[Test]
		public void MakesureRSAPIServiceIsGettingInstalledWithLifestyleTransient()
		{
			//ARRANGE
			IWindsorContainer container = new WindsorContainer();
			
			//ACT
#pragma warning disable 618
			container.Install(new RSAPIServiceInstaller());
#pragma warning restore 618
			var handlers = GetHandlersFor(typeof(IRSAPIService), container);

			//ASSERT
			Assert.IsTrue(handlers.All(x => x.ComponentModel.LifestyleType == LifestyleType.Transient), "RSAPI Service needs to be a transient lifestyle");

		}
		
		private IHandler[] GetAllHandlers(IWindsorContainer container)
		{
			return GetHandlersFor(typeof(object), container);
		}

		private IHandler[] GetHandlersFor(Type type, IWindsorContainer container)
		{
			return container.Kernel.GetAssignableHandlers(type);
		}

	}
}
