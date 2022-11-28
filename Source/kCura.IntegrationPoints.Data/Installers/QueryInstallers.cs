using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Installers
{
    public class QueryInstallers : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<CreateErrorRdoQuery>().ImplementedBy<CreateErrorRdoQuery>().LifestyleTransient());
            container.Register(Component.For<IJobResourceTracker>().ImplementedBy<JobResourceTracker>().LifestyleTransient());
            container.Register(Component.For<IJobTrackerQueryManager>().ImplementedBy<JobTrackerQueryManager>().LifestyleTransient());
            container.Register(Component.For<JobStatistics>().ImplementedBy<JobStatistics>().LifestyleTransient());
            container.Register(Component.For<IJobStatisticsQuery>().ImplementedBy<JobStatisticsQuery>().LifestyleTransient());
            container.Register(Component.For<JobHistoryErrorQuery>().ImplementedBy<JobHistoryErrorQuery>().LifestyleTransient());

            container.Register(Component.For<IObjectTypeQuery>().ImplementedBy<SqlObjectTypeQuery>().LifestyleTransient());
            container.Register(Component.For<IChoiceQuery>().ImplementedBy<ChoiceQuery>().LifestyleTransient());
            container.Register(Component.For<IInstanceSettingRepository>().ImplementedBy<KeplerInstanceSettingRepository>().LifestyleSingleton());
            container.Register(Component.For<GetApplicationBinaries>()
                .UsingFactoryMethod(k =>
                {
                    IEddsDBContext dbContext = new DbContextFactory(k.Resolve<IHelper>()).CreatedEDDSDbContext();
                    return new GetApplicationBinaries(dbContext);
                }).LifestyleTransient());
            container.Register(Component.For<IQueueRepository>().ImplementedBy<QueueRepository>().LifestyleTransient());
            container.Register(Component.For<ISystemEventLoggingService>().ImplementedBy<SystemEventLoggingService>().LifestyleTransient());

            container.Register(Component.For<ISourceProviderArtifactIdByGuidQueryBuilder>().ImplementedBy<SourceProviderArtifactIdByGuidQueryBuilder>().LifestyleSingleton());
            container.Register(Component.For<IDestinationProviderArtifactIdByGuidQueryBuilder>().ImplementedBy<DestinationProviderArtifactIdByGuidQueryBuilder>().LifestyleSingleton());
            container.Register(Component.For<IIntegrationPointsCompletedJobsQueryBuilder>().ImplementedBy<IntegrationPointsCompletedJobsQueryBuilder>().LifestyleSingleton());

            container.Register(Component.For<IDocumentTotalStatistics>().ImplementedBy<DocumentTotalStatistics>().LifestyleTransient());
            container.Register(Component.For<INativeTotalStatistics>().ImplementedBy<NativeTotalStatistics>().LifestyleSingleton());
            container.Register(Component.For<INativeFileSizeStatistics>().ImplementedBy<NativeFileSizeStatistics>().LifestyleSingleton());
            container.Register(Component.For<IImageTotalStatistics>().ImplementedBy<ImageTotalStatistics>().LifestyleSingleton());
            container.Register(Component.For<IImageFileSizeStatistics>().ImplementedBy<ImageFileSizeStatistics>().LifestyleSingleton());
            container.Register(Component.For<IErrorFilesSizeStatistics>().ImplementedBy<ErrorFilesSizeStatistics>().LifestyleSingleton());
            container.Register(Component.For<IRdoStatistics>().ImplementedBy<RdoStatistics>().LifestyleTransient());
        }
    }
}
