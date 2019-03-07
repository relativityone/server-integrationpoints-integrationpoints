using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Agent.Installer.Components;
using kCura.IntegrationPoints.Email;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Installer.Components
{
	[TestFixture]
	public class EmailSenderRegistrationTests
	{
		private IWindsorContainer _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new WindsorContainer();
			_sut.AddEmailSender();
		}

		[Test]
		public void ISmtpConfigurationProvider_ShouldBeRegisteredWithProperLifestyle()
		{
			_sut.Should()
				.HaveRegisteredSingleComponent<ISmtpConfigurationProvider>()
				.Which.Should()
				.BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void ISmtpConfigurationProvider_ShouldBeRegisteredWithProperImplementation()
		{
			_sut.Should().HaveRegisteredProperImplementation<ISmtpConfigurationProvider, InstanceSettingsSmptConfigurationProvider>();
		}

		[Test]
		public void ISmtpConfigurationProvider_ShouldBeResolvedAndNotThrow()
		{
			//arrange
			RegisterInstallerDependencies(_sut);

			//act & assert
			_sut.Should().ResolveWithoutThrowing<ISmtpConfigurationProvider>();
		}

		[Test]
		public void ISmtpClientFactory_ShouldBeRegisteredWithProperLifestyle()
		{
			_sut.Should()
				.HaveRegisteredSingleComponent<ISmtpClientFactory>()
				.Which.Should()
				.BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void ISmtpClientFactory_ShouldBeRegisteredWithProperImplementation()
		{
			_sut.Should().HaveRegisteredProperImplementation<ISmtpClientFactory, SmtpClientFactory>();
		}

		[Test]
		public void ISmtpClientFactory_ShouldBeResolvedAndNotThrow()
		{
			// arrange
			RegisterInstallerDependencies(_sut);

			// act & assert
			_sut.Should().ResolveWithoutThrowing<ISmtpClientFactory>();
		}

		[Test]
		public void IEmailSender_ShouldBeRegisteredWithProperLifestyle()
		{
			_sut.Should()
				.HaveRegisteredSingleComponent<IEmailSender>()
				.Which.Should()
				.BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void IEmailSender_ShouldBeRegisteredWithProperImplementation()
		{
			_sut.Should().HaveRegisteredProperImplementation<IEmailSender, EmailSender>();
		}

		[Test]
		public void IEmailSender_ShouldBeResolvedAndNotThrow()
		{
			// arrange
			RegisterInstallerDependencies(_sut);

			// act & assert
			_sut.Should().ResolveWithoutThrowing<IEmailSender>();
		}

		private void RegisterInstallerDependencies(IWindsorContainer container)
		{
			IRegistration[] dependencies =
			{
				Component.For<IAPILog>().Instance(new Mock<IAPILog>().Object),
				Component.For<IInstanceSettingsBundle>().Instance(new Mock<IInstanceSettingsBundle>().Object)
			};

			container.Register(dependencies);
		}
	}
}
