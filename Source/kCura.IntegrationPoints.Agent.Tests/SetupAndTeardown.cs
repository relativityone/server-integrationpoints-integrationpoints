using kCura.Injection;
using kCura.IntegrationPoints.Injection;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests
{
    [SetUpFixture]
    public class SetupAndTeardown
    {
        private IController _mockController;

        [OneTimeSetUp]
        public void Setup()
        {
            _mockController = Substitute.For<IController>();
            _mockController.GetInjection(null);
            _mockController.Log(null, null);

            InjectionManager.Instance.SetController(_mockController);
        }
    }
}
