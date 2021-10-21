using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Installers.Extensions;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using Moq;
using NUnit.Framework;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Tests.Installers.Extensions
{
	[TestFixture, Category("Unit")]
	public class WindsorContainerExtensionsTests
	{
		[Test]
		public void RegisterWithToggle_ShouldRegisterDisabledImplementation_WhenToggleReadThrows()
		{
			// Arrange
			IWindsorContainer sut = new WindsorContainer();

			// Act
			sut.RegisterWithToggle<IToggleBased, TestToggle, TestToggleEnabled, TestToggleDisabled>(c => c);

			// Assert
			sut.Should()
				.HaveRegisteredProperImplementation<IToggleBased, TestToggleDisabled>();
		}

		[Test]
		public void RegisterWithToggle_ShouldRegisterDisabledImplementation_WhenToggleIsOff()
		{
			// Arrange
			Mock<IToggleProvider> toggleProvider = new Mock<IToggleProvider>();
			toggleProvider.Setup(x => x.IsEnabled<TestToggle>()).Returns(false);

			IWindsorContainer sut = new WindsorContainer();
			sut.Register(Component.For<IToggleProvider>().Instance(toggleProvider.Object));

			// Act
			sut.RegisterWithToggle<IToggleBased, TestToggle, TestToggleEnabled, TestToggleDisabled>(c => c);

			// Assert
			sut.Should()
				.HaveRegisteredProperImplementation<IToggleBased, TestToggleDisabled>();
		}

		[Test]
		public void RegisterWithToggle_ShouldRegisterEnabledImplementation_WhenToggleIsOn()
		{
			// Arrange
			Mock<IToggleProvider> toggleProvider = new Mock<IToggleProvider>();
			toggleProvider.Setup(x => x.IsEnabled<TestToggle>()).Returns(true);

			IWindsorContainer sut = new WindsorContainer();
			sut.Register(Component.For<IToggleProvider>().Instance(toggleProvider.Object));

			// Act
			sut.RegisterWithToggle<IToggleBased, TestToggle, TestToggleEnabled, TestToggleDisabled>(c => c);

			// Assert
			sut.Should()
				.HaveRegisteredProperImplementation<IToggleBased, TestToggleEnabled>();
		}

		[Test]
		public void RegisterWithToggle_ShouldRegisterImplementation_WhenToggleReadThrows()
		{
			// Arrange
			IWindsorContainer sut = new WindsorContainer();

			// Act
			sut.RegisterWithToggle<IToggleBased, TestToggle, TestToggleEnabled, TestToggleDisabled>(c => c.LifestyleTransient());

			// Assert
			sut.Should()
				.HaveRegisteredSingleComponent<IToggleBased>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		private class TestToggleEnabled : IToggleBased { }

		private class TestToggleDisabled : IToggleBased { }

		private interface IToggleBased { }

		private class TestToggle : IToggle { }
	}
}
