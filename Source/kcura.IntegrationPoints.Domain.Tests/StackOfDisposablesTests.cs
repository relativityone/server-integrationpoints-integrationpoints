using System;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Domain.Tests
{
    [TestFixture, Category("Unit")]
    public class StackOfDisposablesTests
    {
        [Test]
        public void ItWorksProperlyOnDisposeWhenEmpty()
        {
            var stackOfDisposables = new StackOfDisposables();

            stackOfDisposables.Dispose();
        }

        [Test]
        public void ItCallsDisposeInProperOrder()
        {
            var disposable1 = Substitute.For<IDisposable>();
            var disposable2 = Substitute.For<IDisposable>();
            var disposable3 = Substitute.For<IDisposable>();

            var stackOfDisposables = new StackOfDisposables();
            stackOfDisposables.Push(disposable1);
            stackOfDisposables.Push(disposable2);
            stackOfDisposables.Push(disposable3);

            stackOfDisposables.Dispose();

            Received.InOrder(() =>
            {
                disposable3.Dispose();
                disposable2.Dispose();
                disposable1.Dispose();
            });
        }

        [Test]
        public void ItCallsDisposeWhenOneOfDisposableIsNull()
        {
            var disposable1 = Substitute.For<IDisposable>();
            var disposable2 = Substitute.For<IDisposable>();

            var stackOfDisposables = new StackOfDisposables();
            stackOfDisposables.Push(disposable1);
            stackOfDisposables.Push(null);
            stackOfDisposables.Push(disposable2);

            stackOfDisposables.Dispose();

            disposable1.Received().Dispose();
            disposable2.Received().Dispose();
        }
    }
}
