using System.Collections.Generic;
using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Services.Installers
{
	public abstract class Installer
	{
		public void Install(IWindsorContainer container, IConfigurationStore store, int workspaceId)
		{
			Mapper.Initialize(x => x.CreateMissingTypeMaps = true);

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