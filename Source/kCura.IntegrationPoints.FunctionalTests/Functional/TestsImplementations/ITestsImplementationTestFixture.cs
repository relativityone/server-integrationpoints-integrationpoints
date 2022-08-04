using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal interface ITestsImplementationTestFixture
    {
        Workspace Workspace { get; }

        void LoginAsStandardUser();
    }
}
