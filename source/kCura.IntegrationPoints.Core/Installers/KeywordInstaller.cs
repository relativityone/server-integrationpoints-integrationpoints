using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.Keywords;

namespace kCura.IntegrationPoints.Core.Installers
{
	public class KeywordInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IKeyword>().ImplementedBy<RipNameKeyword>().LifeStyle.Transient);
			container.Register(Component.For<IKeyword>().ImplementedBy<WorkspaceNameKeyword>().LifeStyle.Transient);
			container.Register(Component.For<IKeyword>().ImplementedBy<ErrorKeyword>().LifeStyle.Transient);
			container.Register(Component.For<KeywordConverter>().ImplementedBy<KeywordConverter>().LifeStyle.Transient);
			container.Register(Component.For<KeywordFactory>().ImplementedBy<KeywordFactory>().LifeStyle.Transient);
		}
	}
}
