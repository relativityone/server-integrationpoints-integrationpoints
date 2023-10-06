using System.Linq;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Utils
{
    public static class Extensions
    {
        public static void ShouldHaveCorrectItemsTransferredUpdateHistory(this JobHistoryFake jobHistoryFake, int from, int to)
        {
            // items transferred can go down due to how IAPI works
            // it reports item transferred, then on item level error it we subtract one transferred item and increment item level errors
            jobHistoryFake.ItemsTransferredHistory.All(x => x >= from && x <= to).Should().BeTrue();
            jobHistoryFake.ItemsTransferredHistory.First().Should().Be(from);
            jobHistoryFake.ItemsTransferredHistory.Last().Should().Be(to);
        }

        public static void ShouldHaveCorrectItemsWithErrorsUpdateHistory(this JobHistoryFake jobHistoryFake, int from, int to)
        {
            jobHistoryFake.ItemsWithErrorsHistory.First().Should().Be(from);
            jobHistoryFake.ItemsWithErrorsHistory.Last().Should().Be(to);
            jobHistoryFake.ItemsWithErrorsHistory.Should().BeInAscendingOrder();
        }
    }
}
