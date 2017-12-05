using System.Collections.Generic;
using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Services.Installers
{
	public abstract class Installer
	{
		static Installer()
		{
			Mapper.Initialize(x => x.CreateMissingTypeMaps = true);
		}

		public void Install(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{			
			RegisterComponents(container, store, workspaceId);

			foreach (var dependency in Dependencies)
			{
				dependency.Install(container, store);
			}
		}

		protected abstract IList<IWindsorInstaller> Dependencies { get; }

		protected abstract void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceId);
	}
}