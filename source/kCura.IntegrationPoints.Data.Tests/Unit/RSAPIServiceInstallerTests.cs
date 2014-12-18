using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using Castle.MicroKernel;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Installers;
using kCura.Vendor.Castle.Windsor;
using NUnit.Framework;
using IWindsorContainer = Castle.Windsor.IWindsorContainer;
using WindsorContainer = Castle.Windsor.WindsorContainer;

namespace kCura.IntegrationPoints.Data.Tests.Unit
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
			container.Install(new RSAPIServiceInstaller());
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
