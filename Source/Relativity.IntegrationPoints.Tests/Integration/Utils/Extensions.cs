using System.Linq;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Utils
{
	public static class Extensions
    {
        public static void ShouldHaveCorrectItemsTransferredUpdateHistory(this JobHistoryTest jobHistoryTest, int from, int to)
        {
            // items transferred can go down due to how IAPI works
            // it reports item transferred, then on item level error it we subtract one transferred item and increment item level errors
            jobHistoryTest.ItemsTransferredHistory.All(x => x >= from && x <= to).Should().BeTrue();
            jobHistoryTest.ItemsTransferredHistory.First().Should().Be(from);
            jobHistoryTest.ItemsTransferredHistory.Last().Should().Be(to);
        }
        
        public static void ShouldHaveCorrectItemsWithErrorsUpdateHistory(this JobHistoryTest jobHistoryTest, int from, int to)
        {
            jobHistoryTest.ItemsWithErrorsHistory.First().Should().Be(from);
            jobHistoryTest.ItemsWithErrorsHistory.Last().Should().Be(to);
            jobHistoryTest.ItemsWithErrorsHistory.Should().BeInAscendingOrder();
        }
    }
}