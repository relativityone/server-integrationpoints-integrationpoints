﻿using System;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.Windsor;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
	public static class WindsorTestingExtensions
	{
		public static IHandler[] GetAllHandlers(this IWindsorContainer container)
		{
			return container.GetHandlersFor<object>();
		}

		public static IHandler[] GetHandlersFor<T>(this IWindsorContainer container)
		{
			return container.Kernel.GetAssignableHandlers(typeof(T));
		}

		public static Type[] GetImplementationTypesFor<T>(this IWindsorContainer container)
		{
			return container.GetHandlersFor<T>()
				.Select(h => h.ComponentModel.Implementation)
				.OrderBy(t => t.Name)
				.ToArray();
		}

		public static IWindsorContainer ChangeLifestyleFromPerWebRequestToTransientInNewRegistrations(this IWindsorContainer container)
		{
			container.Kernel.ComponentModelCreated += model =>
			{
				if (model.LifestyleType == LifestyleType.PerWebRequest)
				{
					model.LifestyleType = LifestyleType.Transient;
				}
			};

			return container;
		}
	}
}
