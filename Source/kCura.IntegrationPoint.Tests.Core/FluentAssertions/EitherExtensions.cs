using kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions;
using LanguageExt;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions
{
    public static class EitherExtensions
    {
        public static EitherAssertions<TLeft, TRight> Should<TLeft, TRight>(this Either<TLeft, TRight> instace)
        {
            return new EitherAssertions<TLeft, TRight>(instace);
        }
    }
}
