﻿using Castle.Core;
using Castle.Windsor;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions
{
	public class WindsorContainerAssertions
	: ReferenceTypeAssertions<IWindsorContainer, WindsorContainerAssertions>
	{
		public WindsorContainerAssertions(IWindsorContainer instance)
		{
			Subject = instance;
		}

		protected override string Context => nameof(IWindsorContainer);

		public AndConstraint<WindsorContainerAssertions> ResolveWithoutThrowing<T>(
			string because = "",
			params object[] becauseArgs
		) where T : class
		{
			bool resolvedNotNull = false;
			Exception thrownException = null;

			try
			{
				T resolved = Subject.Resolve<T>();
				resolvedNotNull = resolved != null;
			}
			catch (Exception ex)
			{
				thrownException = ex;
			}

			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(thrownException == null)
				.FailWith("Expected {context:IWindsorContainer} to resolve {0} without throwing{reason}, but exception was thrown: {1}",
					typeof(T).Name,
					thrownException
				)
				.Then
				.ForCondition(resolvedNotNull)
				.FailWith("Expected {context:IWindsorContainer} to resolve not null value for {0}{reason}, but it was null",
					typeof(T).Name
				);

			return new AndConstraint<WindsorContainerAssertions>(this);
		}

		public AndWhichConstraint<WindsorContainerAssertions, ComponentModel> HaveRegisteredSingleComponent<T>(
			string because = "",
			params object[] becauseArgs
		) where T : class
		{
			ComponentModel registeredComponent = GetRegisteredComponent<T>();

			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(registeredComponent != null)
				.FailWith("Expected {context:IWindsorContainer} to have single component for {0}{reason}, but it hasn't.",
					typeof(T).Name
				);

			return new AndWhichConstraint<WindsorContainerAssertions, ComponentModel>(this, registeredComponent);
		}

		public AndConstraint<ComponentsModelAssertions> HaveRegisteredMultipleComponents<T>(
			string because = "",
			params object[] becauseArgs
		) where T : class
		{
			IList<ComponentModel> registeredComponent = GetRegisteredComponents<T>()?.ToList();

			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(registeredComponent != null && registeredComponent.Any())
				.FailWith("Expected {context:IWindsorContainer} to have multiple components for {0}{reason}, but it hasn't",
					typeof(T).Name
				);

			var componentsAssertions = new ComponentsModelAssertions(registeredComponent);
			return new AndConstraint<ComponentsModelAssertions>(componentsAssertions);
		}

		public AndConstraint<WindsorContainerAssertions> HaveRegisteredProperImplementation<TInterface, TImplementation>(
			string because = "",
			params object[] becauseArgs
		) where TInterface : class
		where TImplementation : class
		{
			Type registeredType = GetImplementationType<TInterface>();

			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(registeredType == typeof(TImplementation))
				.FailWith("Expected {context:IWindsorContainer} to have single component of type {0} for {1}{reason}, but it has {2}.",
					typeof(TImplementation).Name,
					typeof(TInterface).Name,
					registeredType?.Name
				);

			return new AndConstraint<WindsorContainerAssertions>(this);
		}

		private ComponentModel GetRegisteredComponent<T>()
		{
			try
			{
				return GetRegisteredComponents<T>().Single();
			}
			catch (Exception)
			{
				return null;
			}
		}

		private IEnumerable<ComponentModel> GetRegisteredComponents<T>()
		{
			try
			{
				return Subject
					.GetHandlersFor<T>()
					.Select(x => x.ComponentModel);
			}
			catch (Exception)
			{
				return null;
			}
		}


		public Type GetImplementationType<TInterface>() where TInterface : class
		{
			try
			{
				return Subject
					.GetImplementationTypesFor<TInterface>()
					.Single();
			}
			catch
			{
				return null;
			}
		}
	}
}
