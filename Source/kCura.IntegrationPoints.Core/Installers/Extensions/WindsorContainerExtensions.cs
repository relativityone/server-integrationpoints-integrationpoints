using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Installers.Extensions
{
	public static class WindsorContainerExtensions
	{
		/// <summary>
		/// Registration for a single type as a component with the kernel based on <typeparamref name="TToggle"/>
		/// <para><b>Warning: <see cref="IToggleProvider" /> must be registered before use this method.</b></para>
		/// </summary>
		/// <typeparam name="TService"> The service type </typeparam>
		/// <typeparam name="TToggle"> Toggle value which will be evaluated </typeparam>
		/// <typeparam name="TEnabledImpl"> Registered type when toggle is enabled </typeparam>
		/// <typeparam name="TDisabledImpl"> Registered type when toggle is disabled or does not exist </typeparam>
		public static IWindsorContainer RegisterWithToggle<TService, TToggle, TEnabledImpl, TDisabledImpl>(
				this IWindsorContainer container, Func<ComponentRegistration<TService>, ComponentRegistration<TService>> lifestyleFunc)
			where TService : class
			where TToggle : IToggle 
			where TEnabledImpl : TService
			where TDisabledImpl : TService
		{
			bool isToggleEnabled;
			try
			{
				IToggleProvider toggleProvider = container.Resolve<IToggleProvider>();
				isToggleEnabled = toggleProvider.IsEnabled<TToggle>();
			}
			catch (Exception)
			{
				isToggleEnabled = false;
			}

			ComponentRegistration<TService> componentRegistration;
			if (isToggleEnabled)
			{
				componentRegistration = Component.For<TService>().ImplementedBy<TEnabledImpl>();
			}
			else
			{
				componentRegistration = Component.For<TService>().ImplementedBy<TDisabledImpl>();
			}

			componentRegistration = lifestyleFunc(componentRegistration);

			container.Register(componentRegistration);

			return container;
		}
	}
}
