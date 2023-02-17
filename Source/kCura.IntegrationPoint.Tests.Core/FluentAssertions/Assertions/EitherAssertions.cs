using FluentAssertions;
using FluentAssertions.Execution;
using LanguageExt;
using System.Linq;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions
{
    public class EitherAssertions<TLeft, TRight>
    {
        public Either<TLeft, TRight> Subject { get; }

        public EitherAssertions(Either<TLeft, TRight> value)
        {
            Subject = value;
        }

        public AndConstraint<EitherAssertions<TLeft, TRight>> BeRight(
            string because,
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(Subject.IsRight)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Either to be in a right state{reason}, but found {0}", Subject.State);

            return new AndConstraint<EitherAssertions<TLeft, TRight>>(this);
        }

        public AndConstraint<EitherAssertions<TLeft, TRight>> BeLeft(
            TLeft expectedValue,
            string because,
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(Subject.IsLeft)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context} to be in a left state{reason}, but found {0}", Subject.State)
                .Then
                .ForCondition(Subject.LeftAsEnumerable().Single().Equals(expectedValue))
                .BecauseOf(because, becauseArgs)
                .FailWith(
                    "Expected Either to have left value{0}{reason}, but found {1}",
                    expectedValue,
                    Subject.LeftAsEnumerable().Single()
                );

            return new AndConstraint<EitherAssertions<TLeft, TRight>>(this);
        }
    }
}
