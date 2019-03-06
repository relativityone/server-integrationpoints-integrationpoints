using Castle.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions
{
	public class ComponentsModelAssertions
		: ReferenceTypeAssertions<IEnumerable<ComponentModel>, ComponentsModelAssertions>
	{
		public ComponentsModelAssertions(IEnumerable<ComponentModel> instance)
		{
			Subject = instance;
		}

		protected override string Context => nameof(IEnumerable<ComponentModel>);

		public AndWhichConstraint<ComponentsModelAssertions, ComponentModel> OneOfThemWithImplementation<T>(
			string because = "",
			params object[] becauseArgs)
		{
			ComponentModel foundComponent = Subject.FirstOrDefault(x => x.Implementation == typeof(T));

			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(foundComponent != null)
				.FailWith("Implementation {0} expected to be registered{reason}, but it wasn't.",
					typeof(T).Name
				);

			return new AndWhichConstraint<ComponentsModelAssertions, ComponentModel>(this, foundComponent);
		}
	}
}
