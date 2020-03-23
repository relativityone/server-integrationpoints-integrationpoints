using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
    public class FileBillingFlagValidator
    {
        private ITestHelper Helper { get; set; }
        private int workspaceArtifactID { get; set; }

        private DataTable fileDataTable => Helper.GetDBContext(workspaceArtifactID)
            .ExecuteSqlStatementAsDataTable("SELECT * FROM [File]");

        public FileBillingFlagValidator(ITestHelper testHelper, int targetWorkspaceArtifactID)
        {
            Helper = testHelper;
            workspaceArtifactID = targetWorkspaceArtifactID;
        }

        public void AssertFiles(bool expectBillable)
        {
            IEnumerable<FileRow> fileRows = GetFiles();

            fileRows.Should().NotBeEmpty()
                .And.OnlyContain(x => x.InRepository == expectBillable && x.Billable == expectBillable);
        }

        private IEnumerable<FileRow> GetFiles()
        {
            return fileDataTable.Select().Select(x => new FileRow
            {
                InRepository = (bool) x["InRepository"],
                Billable = (bool) x["Billable"]
            });
        }

        private class FileRow
        {
            public bool InRepository { get; set; }
            public bool Billable { get; set; }
        }
    }
}