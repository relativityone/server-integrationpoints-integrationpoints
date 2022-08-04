using System;
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
        public ComponentsModelAssertions(IEnumerable<ComponentModel> subject)
        {
            Subject = subject;
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

        public AndConstraint<ComponentsModelAssertions> AllWithLifestyle(
            LifestyleType lifestyleType,
            string because = "",
            params object[] becauseArgs)
        {
            IEnumerable<string> invalidComponentsNames = Subject
                .Where(component => component.LifestyleType != lifestyleType)
                .Select(component => component.Implementation.Name);

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!invalidComponentsNames.Any())
                .FailWith("All components expected to have {0} lifestyle{reason}, but {1} have different lifestyle",
                    lifestyleType,
                    invalidComponentsNames
                );

            return new AndConstraint<ComponentsModelAssertions>(this);
        }

        public AndConstraint<ComponentsModelAssertions> AllExposeThemselvesAsService(
            string because = "",
            params object[] becauseArgs)
        {
            IEnumerable<string> invalidComponentsNames = Subject
                .Where(component => component.Services.Single() != component.Implementation)
                .Select(component => component.Implementation.Name);

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!invalidComponentsNames.Any())
                .FailWith("All components expected to expose themselves as service{reason}, but {0} haven't",
                    invalidComponentsNames
                );

            return new AndConstraint<ComponentsModelAssertions>(this);
        }

        public AndConstraint<ComponentsModelAssertions> AllRegisteredInFollowingOrder(
            IEnumerable<Type> expectedOrder,
            string because = "",
            params object[] becauseArgs)
        {
            IEnumerable<Type> actualImplementations = Subject
                .Select(x => x.Implementation);

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(expectedOrder.SequenceEqual(actualImplementations))
                .FailWith("All components expected to be registered in the correct order{reason}");

            return new AndConstraint<ComponentsModelAssertions>(this);
        }
    }
}
