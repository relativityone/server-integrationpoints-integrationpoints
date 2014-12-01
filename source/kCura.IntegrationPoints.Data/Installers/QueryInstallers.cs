using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Data.Installers
{
	public class QueryInstallers : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<Queries.CreateErrorRdo>().ImplementedBy<Queries.CreateErrorRdo>());
			container.Register(Component.For<RelativityRdoQuery>().ImplementedBy<RelativityRdoQuery>());
		}
	}
}
