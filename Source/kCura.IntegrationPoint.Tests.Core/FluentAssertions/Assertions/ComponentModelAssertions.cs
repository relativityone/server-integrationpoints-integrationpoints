using Castle.Core;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions
{
	public class ComponentModelAssertions
		: ReferenceTypeAssertions<ComponentModel, ComponentModelAssertions>
	{
		private readonly string _nameOfComponent;

		public ComponentModelAssertions(ComponentModel instance, string nameOfComponent)
		{
			Subject = instance;
			_nameOfComponent = nameOfComponent;
		}

		protected override string Context => nameof(ComponentModel);

		public ComponentModelAssertions WithLifestyle(
			LifestyleType expectedLifestyle, 
			string because = "",
			params object[] becauseArgs)
		{
			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(Subject.LifestyleType == expectedLifestyle)
				.FailWith("Component {0} expected to have {1} lifestyle{reason}, but it has {2}.",
					_nameOfComponent, 
					expectedLifestyle, 
					Subject.LifestyleType
				);

			return new ComponentModelAssertions(Subject, _nameOfComponent);
		}

		public ComponentModelAssertions WithName(
			string expectedName,
			string because = "",
			params object[] becauseArgs)
		{
			Execute.Assertion
				.BecauseOf(because, becauseArgs)
				.ForCondition(Subject.Name == expectedName)
				.FailWith("Component {0} expected to have {1} name{reason}, but it has {2}.",
					_nameOfComponent,
					expectedName,
					Subject.Name
				);

			return new ComponentModelAssertions(Subject, _nameOfComponent);
		}
	}
}
