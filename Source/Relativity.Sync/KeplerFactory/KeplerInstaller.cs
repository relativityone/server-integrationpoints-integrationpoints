using Autofac;
using Relativity.Sync.Authentication;

namespace Relativity.Sync.KeplerFactory
{
	internal sealed class KeplerInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<OAuth2ClientFactory>().As<IOAuth2ClientFactory>();
			builder.RegisterType<OAuth2TokenGenerator>().As<IAuthTokenGenerator>();

			builder.RegisterType<TokenProviderFactoryFactory>().As<ITokenProviderFactoryFactory>();
			builder.RegisterType<ServiceFactoryForUser>()
				.As<ISourceServiceFactoryForUser>()
				.As<IDestinationServiceFactoryForUser>();
			builder.RegisterType<ServiceFactoryForAdmin>()
				.As<ISourceServiceFactoryForAdmin>()
				.As<IDestinationServiceFactoryForAdmin>();

			builder.RegisterType<DynamicProxyFactory>().As<IDynamicProxyFactory>();

			builder.RegisterType<ServiceFactoryFactory>().As<IServiceFactoryFactory>();
		}
	}
}