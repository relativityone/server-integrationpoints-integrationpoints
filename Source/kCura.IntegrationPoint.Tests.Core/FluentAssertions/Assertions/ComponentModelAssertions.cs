﻿using Castle.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions
{
	public class ComponentModelAssertions : ReferenceTypeAssertions<ComponentModel, ComponentModelAssertions>
	{
		public ComponentModelAssertions(ComponentModel instance)
		{
			Subject = instance;
		}

		protected override string Identifier { get; }

		public AndConstraint<ComponentModelAssertions> BeRegisteredWithLifestyle(
			LifestyleType expectedLifestyle,
			string because = "",
			params object[] becauseArgs)
		{
			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(Subject.LifestyleType == expectedLifestyle)
				.FailWith("Component {0} expected to have {1} lifestyle{reason}, but it has {2}.",
					Subject.Implementation.Name,
					expectedLifestyle,
					Subject.LifestyleType
				);

			return new AndConstraint<ComponentModelAssertions>(this);
		}

		public AndConstraint<ComponentModelAssertions> BeRegisteredWithName(
			string expectedName,
			string because = "",
			params object[] becauseArgs)
		{
			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(Subject.Name == expectedName)
				.FailWith("Component {0} expected to have {1} name{reason}, but it has {2}.",
					Subject.Implementation.Name,
					expectedName,
					Subject.Name
				);

			return new AndConstraint<ComponentModelAssertions>(this);
		}
	}
}
