using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    public sealed class ValidationExecutionConstrainsTests
    {
        private ValidationExecutionConstrains _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ValidationExecutionConstrains();
        }

        [Test]
        public async Task ItShouldAlwaysCanExecute()
        {
            // act
            bool canExecute = await _sut.CanExecuteAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.IsTrue(canExecute);
        }
    }
}