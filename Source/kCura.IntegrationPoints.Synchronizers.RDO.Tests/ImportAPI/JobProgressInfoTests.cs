using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.ImportAPI
{
    [TestFixture, Category("Unit")]
    public class JobProgressInfoTests
    {
        [Test]
        public void ItShouldReturnZeroAfterCreation()
        {
            // arrange
            var sut = new JobProgressInfo();

            // assert
            Assert.AreEqual(0, sut.NumberOfItemsTransferred);
            Assert.AreEqual(0, sut.NumberOfItemsErrored);
        }

        [Test]
        public void ItShouldReturnOneItemsTransferred()
        {
            // arrange
            var sut = new JobProgressInfo();
            sut.ItemTransferred();

            // assert
            Assert.AreEqual(1, sut.NumberOfItemsTransferred);
            Assert.AreEqual(0, sut.NumberOfItemsErrored);
        }

        [Test]
        public void ItShouldReturnOneItemsErrored()
        {
            // arrange
            var sut = new JobProgressInfo();
            sut.ItemErrored();

            // assert
            Assert.AreEqual(-1, sut.NumberOfItemsTransferred);
            Assert.AreEqual(1, sut.NumberOfItemsErrored);
        }

        [Test]
        public void ItShouldReturnZeroItemsAfterReset()
        {
            // arrange
            var sut = new JobProgressInfo();
            sut.ItemTransferred();

            // act
            sut.Reset();

            // assert
            Assert.AreEqual(0, sut.NumberOfItemsTransferred);
            Assert.AreEqual(0, sut.NumberOfItemsErrored);
        }

        [Test]
        public void ItShouldDecrementItemsTransferredAfterError()
        {
            // arrange
            var sut = new JobProgressInfo();
            sut.ItemTransferred();
            sut.ItemErrored();

            // assert
            Assert.AreEqual(0, sut.NumberOfItemsTransferred);
            Assert.AreEqual(1, sut.NumberOfItemsErrored);
        }

        [Test]
        public void ItShouldBeValidWhenMoreTransferredThenErrored()
        {
            // arrange
            var sut = new JobProgressInfo();
            sut.ItemTransferred();
            sut.ItemErrored();
            sut.ItemErrored();
            sut.ItemTransferred();
            sut.ItemTransferred();

            // assert
            Assert.IsTrue(sut.IsValid());
        }

        [Test]
        public void ItShouldBeInvalidWhenMoreErrorsThenTransferred()
        {
            // arrange
            var sut = new JobProgressInfo();
            sut.ItemTransferred();
            sut.ItemErrored();
            sut.ItemErrored();

            // assert
            Assert.IsFalse(sut.IsValid());
        }
    }
}
