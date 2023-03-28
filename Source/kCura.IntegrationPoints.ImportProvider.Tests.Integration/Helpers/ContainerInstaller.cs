using System;
using System.Net;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.ImportProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataExchange;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration.Helpers
{
    public static class ContainerInstaller
    {
        public static WindsorContainer CreateContainer()
        {
            var windsorContainer = new WindsorContainer();
            windsorContainer.Kernel.Resolver.AddSubResolver(new CollectionResolver(windsorContainer.Kernel));

            windsorContainer.Register(Component.For<IHelper>().Instance(Substitute.For<IHelper>()).LifestyleTransient());

            RegisterJobManagers(windsorContainer);
            RegisterLoggingClasses(windsorContainer);
            RegisterSerializer(windsorContainer);
            RegisterConfig(windsorContainer);
            RegisterDomainClasses(windsorContainer);
            RegisterSyncClasses(windsorContainer);
            RegisterCredentialProvider(windsorContainer);
            RegisterParserClasses(windsorContainer);

            RegisterImportTestCases(windsorContainer);

            return windsorContainer;
        }

        private static void RegisterImportTestCases(WindsorContainer windsorContainer)
        {
            windsorContainer.Register(
                Classes.FromThisAssembly()
                    .IncludeNonPublicTypes()
                    .BasedOn<IImportTestCase>()
                    .Unless(type => Attribute.IsDefined(type, typeof(IgnoreAttribute)))
                    .WithServiceAllInterfaces()
                    .AllowMultipleMatches());
        }

        private static void RegisterJobManagers(WindsorContainer windsorContainer)
        {
            var jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
            windsorContainer.Register(Component.For<IJobHistoryErrorService>().Instance(jobHistoryErrorService).LifestyleSingleton());
        }

        private static void RegisterLoggingClasses(WindsorContainer windsorContainer)
        {
            windsorContainer.Register(Component.For<IAPILog>().Instance(Substitute.For<IAPILog>()).LifestyleSingleton());
            windsorContainer.Register(Component.For(typeof(ILogger<>)).ImplementedBy(typeof(Logger<>)));
        }

        private static void RegisterSerializer(WindsorContainer windsorContainer)
        {
            windsorContainer.Register(Component.For<ISerializer>().Instance(IntegrationPointSerializer.CreateWithoutLogger()));
        }

        public static void RegisterSyncClasses(WindsorContainer windsorContainer)
        {
            windsorContainer.Register(Component.For<IRetryHandlerFactory>().ImplementedBy<RetryHandlerFactory>().LifestyleTransient());
            windsorContainer.Register(Component.For<IRelativityFieldQuery>().ImplementedBy<RelativityFieldQuery>().LifestyleTransient());
            windsorContainer.Register(Component.For<IImportApiBuilder>().ImplementedBy<ImportApiBuilder>().LifestyleTransient());
            windsorContainer.Register(Component.For<IRelativityTokenProvider>().ImplementedBy<RelativityTokenProvider>().LifestyleTransient());
            windsorContainer.Register(Component.For<IImportApiFactory>().ImplementedBy<ImportApiFactory>().LifestyleTransient());
            windsorContainer.Register(Component.For<IImportJobFactory>().ImplementedBy<ImportJobFactory>().LifestyleTransient());

        }

        private static void RegisterConfig(WindsorContainer windsorContainer)
        {
            IWebApiConfig webApiConfig = Substitute.For<IWebApiConfig>();
            webApiConfig.GetWebApiUrl.Returns(SharedVariables.RelativityWebApiUrl);
            windsorContainer.Register(Component.For<IWebApiConfig>().Instance(webApiConfig));
        }

        private static void RegisterCredentialProvider(WindsorContainer windsorContainer)
        {
            IWebApiLoginService credProvider = Substitute.For<IWebApiLoginService>();
            NetworkCredential temp = new NetworkCredential(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
            NetworkCredential authorizedCredential = temp.GetCredential(SharedVariables.RelativityHostAddress, 8990, "password");
            credProvider.Authenticate(Arg.Any<CookieContainer>()).Returns(authorizedCredential);
            windsorContainer.Register(Component.For<IWebApiLoginService>().Instance(credProvider));
        }

        private static void RegisterParserClasses(WindsorContainer windsorContainer)
        {
            windsorContainer.Register(Component.For<IFieldParserFactory>().ImplementedBy<FieldParserFactory>().LifestyleTransient());
            windsorContainer.Register(Component.For<IWinEddsLoadFileFactory>().ImplementedBy<WinEddsLoadFileFactory>().LifestyleTransient());
            windsorContainer.Register(Component.For<IWinEddsBasicLoadFileFactory>().ImplementedBy<WinEddsBasicLoadFileFactory>().LifestyleTransient());
            windsorContainer.Register(Component.For<IWinEddsFileReaderFactory>().ImplementedBy<WinEddsFileReaderFactory>());
            windsorContainer.Register(Component.For<IDataReaderFactory>().ImplementedBy<DataReaderFactory>().LifestyleTransient());
        }

        private static void RegisterDomainClasses(WindsorContainer windsorContainer)
        {
            windsorContainer.Register(Component.For<IAuthTokenGenerator>().ImplementedBy<ClaimsTokenGenerator>().LifestyleTransient());
            windsorContainer.Register(Component.For<IInstanceSettingsManager>().Instance(Substitute.For<IInstanceSettingsManager>()));
            windsorContainer.Register(Component.For<IFederatedInstanceManager>().Instance(Substitute.For<IFederatedInstanceManager>()));
            windsorContainer.Register(Component.For<IMessageService>().Instance(Substitute.For<IMessageService>()));
        }
    }
}
