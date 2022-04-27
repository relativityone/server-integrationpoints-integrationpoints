using FluentAssertions;
using Moq;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Integration;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsAssertions
{
    public class BillingFlagAssertion
    {        
        private int _workspaceArtifactID { get; set; }
       
        private SqlConnection connection => SqlHelper.CreateConnectionFromAppConfig(_workspaceArtifactID);
        private DataTable fileDataTable => SqlHelper.ExecuteSqlStatementAsDataTable(connection, "SELECT * FROM [File]");
          
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
            return fileDataTable.Select().Select(x => new FileRow
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
