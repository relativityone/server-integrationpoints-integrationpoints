using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsAssertions
{
    public static class BillingFlagAssertion
    {
        public static void AssertFiles(int workspaceArtifactId, bool expectBillable)
        {
            IEnumerable<FileRow> fileRows = GetFiles(workspaceArtifactId);

            fileRows.Should().NotBeEmpty()
                .And.OnlyContain(x => x.InRepository == expectBillable && x.Billable == expectBillable);
        }

        private static IEnumerable<FileRow> GetFiles(int workspaceArtifactId)
        {
            DataTable dataTable = SqlHelper.ExecuteSqlStatementAsDataTable(workspaceArtifactId, "SELECT * FROM [File]");
            return dataTable.Select().Select(x => new FileRow
            {
                InRepository = (bool)x["InRepository"],
                Billable = (bool)x["Billable"]
            });
        }

        private class FileRow
        {
            public bool InRepository { get; set; }

            public bool Billable { get; set; }
        }
    }
}
