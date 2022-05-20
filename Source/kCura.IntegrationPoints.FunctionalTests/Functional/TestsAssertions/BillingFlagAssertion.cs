using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsAssertions
{
    public class BillingFlagAssertion
    {
        private DataTable _fileDataTable;
        private int _workspaceArtifactID { get; set; }

        public BillingFlagAssertion(int targetWorkspaceArtifactID)
        {
            _workspaceArtifactID = targetWorkspaceArtifactID;
        }

        public void AssertFiles(bool expectBillable)
        {
            IEnumerable<FileRow> fileRows = GetFiles();

            fileRows.Should().NotBeEmpty()
                .And.OnlyContain(x => x.InRepository == expectBillable && x.Billable == expectBillable);
        }

        private IEnumerable<FileRow> GetFiles()
        {
            _fileDataTable = SqlHelper.ExecuteSqlStatementAsDataTable(_workspaceArtifactID, "SELECT * FROM [File]");
            return _fileDataTable.Select().Select(x => new FileRow
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
