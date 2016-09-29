﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Data.Installers
{
	[Obsolete("This class is obsolete as it does not conform to our usage of the Composition Root.")]
	public class QueryInstallers : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<CreateCustodianManagerResourceTable>().ImplementedBy<CreateCustodianManagerResourceTable>().LifestyleTransient());
			container.Register(Component.For<CreateErrorRdo>().ImplementedBy<CreateErrorRdo>().LifestyleTransient());
			container.Register(Component.For<GetApplicationGuid>().ImplementedBy<GetApplicationGuid>().LifestyleTransient());
			container.Register(Component.For<GetJobCustodianManagerLinks>().ImplementedBy<GetJobCustodianManagerLinks>().LifestyleTransient());
			container.Register(Component.For<GetJobsCount>().ImplementedBy<GetJobsCount>().LifeStyle.Transient);
			container.Register(Component.For<GetSavedSearchesQuery>().ImplementedBy<GetSavedSearchesQuery>().LifeStyle.Transient);
			container.Register(Component.For<GetWorkspacesQuery>().ImplementedBy<GetWorkspacesQuery>().LifeStyle.Transient);
			container.Register(Component.For<JobResourceTracker>().ImplementedBy<JobResourceTracker>().LifeStyle.Transient);
			container.Register(Component.For<JobStatistics>().ImplementedBy<JobStatistics>().LifeStyle.Transient);

			container.Register(Component.For<IObjectTypeQuery>().ImplementedBy<SqlObjectTypeQuery>().LifestyleTransient());
			container.Register(Component.For<RSAPIRdoQuery>().ImplementedBy<RSAPIRdoQuery>().LifeStyle.Transient);

			container.Register(Component.For<IChoiceQuery>().ImplementedBy<ChoiceQuery>().LifeStyle.Transient);
            container.Register(Component.For<IFileQuery>().ImplementedBy<FileQuery>().LifeStyle.Transient);
        }
    }
}
