using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	internal class EventHandlerInstaller : IWindsorInstaller
	{
		private readonly IEHContext _context;

		public EventHandlerInstaller(IEHContext context)
		{
			_context = context;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IRsapiClientFactory>().Instance(new RsapiClientFactory(_context.Helper)).LifestyleSingleton());
			container.Register(Component.For<IEHContext>().Instance(_context).LifestyleSingleton());
			container.Register(Component.For<IHelper>().Instance(_context.Helper).LifestyleSingleton());
			container.Register(Component.For<IRSAPIService>().UsingFactoryMethod(k =>
			{
				IRSAPIServiceFactory serviceFactory = k.Resolve<IRSAPIServiceFactory>();
				IEHContext context = k.Resolve<IEHContext>();
				return serviceFactory.Create(context.Helper.GetActiveCaseID());
			}).LifestyleTransient());
			container.Register(Component.For<IServiceContextHelper>().UsingFactoryMethod(k =>
			{
				IEHContext context = k.Resolve<IEHContext>();
				IRsapiClientFactory clientFactory = k.Resolve<IRsapiClientFactory>();
				return new ServiceContextHelperForEventHandlers(context.Helper, context.Helper.GetActiveCaseID(), clientFactory);
			}).LifestyleSingleton());
			container.Register(Component.For<IWorkspaceDBContext>().UsingFactoryMethod(k =>
			{
				var context = container.Resolve<IEHContext>();
				return new WorkspaceContext(context.Helper.GetDBContext(context.Helper.GetActiveCaseID()));
			}).LifestyleTransient());
			container.Register(Component.For<IIntegrationPointProviderValidator>()
				.Instance(new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(),
					container.Resolve<IIntegrationPointSerializer>())).LifestyleSingleton());
			container.Register(Component.For<IIntegrationPointPermissionValidator>()
				.Instance(new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(),
					container.Resolve<IIntegrationPointSerializer>())).LifestyleSingleton());
			container.Register(Component.For<IAuthTokenGenerator>().UsingFactoryMethod(kernel =>
			{
				IEHContext context = kernel.Resolve<IEHContext>();
				IOAuth2ClientFactory oauth2ClientFactory = kernel.Resolve<IOAuth2ClientFactory>();
				ITokenProviderFactoryFactory tokenProviderFactory = kernel.Resolve<ITokenProviderFactoryFactory>();
				CurrentUser contextUser = new CurrentUser {ID = context.Helper.GetAuthenticationManager().UserInfo.ArtifactID};

				return new OAuth2TokenGenerator(context.Helper, oauth2ClientFactory, tokenProviderFactory, contextUser);
			}).LifestyleTransient());
			container.Register(Component.For<IRSAPIClient>().UsingFactoryMethod(k =>
			{
				IEHContext context = k.Resolve<IEHContext>();
				IRsapiClientFactory clientFactory = k.Resolve<IRsapiClientFactory>();
				return clientFactory.CreateAdminClient(context.Helper.GetActiveCaseID());
			}));
			container.Register(Component.For<DeleteIntegrationPointCommand>().ImplementedBy<DeleteIntegrationPointCommand>().LifestyleTransient());
			container.Register(Component.For<PreCascadeDeleteIntegrationPointCommand>().ImplementedBy<PreCascadeDeleteIntegrationPointCommand>().LifestyleTransient());
			container.Register(Component.For<CreateTenantIdForSecretStoreCommand>().ImplementedBy<CreateTenantIdForSecretStoreCommand>().LifestyleTransient());
			container.Register(Component.For<RemoveAgentJobLogTableCommand>().ImplementedBy<RemoveAgentJobLogTableCommand>().LifestyleTransient());
			container.Register(Component.For<ImportNativeFileCopyModeUpdater>().ImplementedBy<ImportNativeFileCopyModeUpdater>().LifestyleTransient());
			container.Register(Component.For<SetImportNativeFileCopyModeCommand>().ImplementedBy<SetImportNativeFileCopyModeCommand>().LifestyleTransient());
			container.Register(Component.For<IIntegrationPointSecretDelete>().UsingFactoryMethod(k => IntegrationPointSecretDeleteFactory.Create(k.Resolve<IEHContext>().Helper))
				.LifestyleTransient());
			container.Register(Component.For<ICorrespondingJobDelete>().ImplementedBy<CorrespondingJobDelete>().LifestyleTransient());
			container.Register(Component.For<IPreCascadeDeleteEventHandlerValidator>().ImplementedBy<PreCascadeDeleteEventHandlerValidator>().LifestyleTransient());
			container.Register(Component.For<IArtifactsToDelete>().ImplementedBy<ArtifactsToDelete>().LifestyleTransient());
			container.Register(Component.For<ICreateTenantIdForSecretStore>().ImplementedBy<CreateTenantIdForSecretStore>().LifestyleTransient());
			container.Register(Component.For<ITenantForSecretStoreCreationValidator>().ImplementedBy<TenantForSecretStoreCreationValidator>().LifestyleTransient());
		}
	}
}