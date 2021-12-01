using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.System
{
    [TestFixture]
    internal class NonDocumentObjectImportTest: SystemTest
    {
        //protected int SourceWorkspaceId;
        //protected int DestinationWorkspaceId;
        //protected IImportJobFactory importJobFactory;

        //private static IEnumerable<TestCaseData> TestCases =>
        //    new List<TestCaseData>
        //        {
        //            new TestCaseData("Simple-Transfer-Small-1"),
        //            new TestCaseData("Simple-Transfer-Small-2"),
        //            new TestCaseData("Simple-Transfer-Medium-1"),
        //            new TestCaseData("Simple-Transfer-Medium-2"),
        //            new TestCaseData("Simple-Transfer-Large-1"),
        //            new TestCaseData("Simple-Transfer-Large-2"),
        //        };

        //protected override async Task ChildSuiteSetup()
        //{
        //    await base.ChildSuiteSetup();

        //    importJobFactory = new ImportJobFactory(null, null, null, null, FakeHelper.CreateSyncJobParameters(), null);

        //    WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
        //    SourceWorkspaceId = sourceWorkspace.ArtifactID;

        //    WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
        //    DestinationWorkspaceId = destinationWorkspace.ArtifactID;
        //}

        //public NonDocumentObjectImportTest()
        //{

        //}

        //[IdentifiedTest("25b723da-82fe-4f56-ae9f-4a8b2a4d60f4")]
        //[TestType.MainFlow]
        //public void sth_should_dosth()
        //{

        //}
    }
}
