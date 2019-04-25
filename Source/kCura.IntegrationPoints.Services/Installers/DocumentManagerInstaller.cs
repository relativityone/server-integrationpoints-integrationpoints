﻿using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;

namespace kCura.IntegrationPoints.Services.Installers
{
	public class DocumentManagerInstaller : Installer
	{
		public DocumentManagerInstaller()
		{
			Dependencies = new List<IWindsorInstaller>();
		}

		protected override IList<IWindsorInstaller> Dependencies { get; }

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
		{
			container.Register(Component.For<ISecretCatalogFactory>().ImplementedBy<DefaultSecretCatalogFactory>().LifestyleTransient());
			container.Register(Component.For<ISecretManagerFactory>().ImplementedBy<SecretManagerFactory>().LifestyleTransient());
			container.Register(Component.For<IRelativityObjectManagerFactory>().ImplementedBy<RelativityObjectManagerFactory>().LifestyleTransient());
			container.Register(Component.For<IDocumentRepository>().ImplementedBy<DocumentRepository>().LifestyleTransient());
		}
	}
}