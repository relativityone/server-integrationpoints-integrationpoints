using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Email;

namespace kCura.IntegrationPoints.Agent.Installer.Components
{
    internal static class EmailSenderRegistration
    {
        /// <summary>
        /// It registers <see cref="IEmailSender"/> in a container
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IWindsorContainer AddEmailSender(this IWindsorContainer container)
        {
            container.Register(Component
                .For<ISmtpConfigurationProvider>()
                .ImplementedBy<InstanceSettingsSmptConfigurationProvider>()
                .LifestyleTransient()
            );
            container.Register(Component
                .For<ISmtpClientFactory>()
                .ImplementedBy<SmtpClientFactory>()
                .LifestyleTransient()
            );
            container.Register(Component
                .For<IEmailSender>()
                .ImplementedBy<EmailSender>()
                .LifestyleTransient()
            );

            return container;
        }
    }
}
