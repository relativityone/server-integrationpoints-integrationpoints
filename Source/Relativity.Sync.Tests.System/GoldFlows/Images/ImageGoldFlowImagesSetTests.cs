using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
    internal sealed class ImageGoldFlowImagesSetTests : ImageGoldFlowTestsBase
    {
        public ImageGoldFlowImagesSetTests() : base(Dataset.Images, expectedItemsForRetry: 3)
        {
        }

        [Test]
        [TestType.MainFlow]
        [TestType.Critical]
        public override Task SyncJob_Should_SyncImages()
        {
            return base.SyncJob_Should_SyncImages();
        }

        [IdentifiedTest("084CD14B-2D5C-4B7A-86DF-9C3C4606410B")]
        [TestType.MainFlow]
        [TestType.Critical]
        public override Task SyncJob_Should_RetryImages()
        {
            return base.SyncJob_Should_RetryImages();
        }
    }
}
