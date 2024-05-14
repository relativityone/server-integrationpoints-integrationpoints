using System;
using Autofac;
using Autofac.Core;
using Relativity.Sync.Authentication;
using Relativity.Sync.Executors.Tagging;
using Relativity.Sync.Kepler.Document;
using Relativity.Sync.Kepler.Snapshot;

namespace Relativity.Sync.KeplerFactory
{
    internal sealed class KeplerInstaller : IInstaller
    {
        public void Install(ContainerBuilder builder)
        {
            builder.RegisterType<OAuth2ClientFactory>().As<IOAuth2ClientFactory>();
            builder.RegisterType<OAuth2TokenGenerator>().As<IAuthTokenGenerator>()
                .WithParameter(new ResolvedParameter((pi, ctx) => pi.ParameterType == typeof(Uri), (pi, ctx) => ctx.Resolve<IRelativityServices>().AuthenticationUri));

            builder.RegisterType<TokenProviderFactoryFactory>().As<ITokenProviderFactoryFactory>();

            builder.RegisterType<ServiceFactoryForUser>()
                .As<ISourceServiceFactoryForUser>()
                .As<IDestinationServiceFactoryForUser>()
                .As<IServiceFactoryForUser>()
                .SingleInstance();

            builder.RegisterType<ServiceFactoryForAdmin>()
                .As<ISourceServiceFactoryForAdmin>()
                .As<IDestinationServiceFactoryForAdmin>()
                .As<IServiceFactoryForAdmin>()
                .SingleInstance();

            builder.RegisterType<DynamicProxyFactory>().As<IDynamicProxyFactory>().SingleInstance();

            builder.RegisterType<ServiceFactoryFactory>().As<IServiceFactoryFactory>();
            builder.RegisterType<SnapshotRepository>().As<ISnapshotRepository>();
            builder.RegisterType<TaggingRepository>().As<ITaggingRepository>();
            builder.RegisterType<DocumentRepository>().As<IDocumentRepository>();
            builder.RegisterType<ProxyFactoryDocument>().As<IProxyFactoryDocument>();
        }
    }
}
