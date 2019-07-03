﻿using System.Collections.Generic;
using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using Relativity.API;

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
			RegisterCommonComponents(container);
			RegisterComponents(container, store, workspaceId);

			foreach (var dependency in Dependencies)
			{
				dependency.Install(container, store);
			}
		}

		private void RegisterCommonComponents(IWindsorContainer container)
		{
			container.Install(new InfrastructureInstaller());
#pragma warning disable CS0618 // Type or member is obsolete REL-292860
			container.Register(Component
				.For<IHelper, IServiceHelper>()
				.Instance(global::Relativity.API.Services.Helper)
			);
#pragma warning restore CS0618 // Type or member is obsolete
			container.Register(Component
				.For<ILazyComponentLoader>()
				.ImplementedBy<LazyOfTComponentLoader>()
			);
		}

		protected abstract IList<IWindsorInstaller> Dependencies { get; }

		protected abstract void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID);
	}
}