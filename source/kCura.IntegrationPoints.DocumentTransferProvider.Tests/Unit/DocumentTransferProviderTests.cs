using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
  [TestFixture]
  public class DocumentTransferProviderTests
  {
    #region Read

    [Test]
    public void GetEmailBodyData_HasWorkspace_CorrectlyFormatedOutput()
    {
      //ARRANGE
      int workspaceId = 1111111;
      var helper = NSubstitute.Substitute.For<IHelper>();
      Workspace workspace = new Workspace(workspaceId) { Name = "My Test workspace" };

      IEmailBodyData mockDocumentTransferProvider = new mockDocumentTransferProvider(helper, workspace);

      var settings = new DocumentTransferSettings { SourceWorkspaceArtifactId = workspaceId };
      var options = JsonConvert.SerializeObject(settings);

      //ACT
      var returnedString = mockDocumentTransferProvider.GetEmailBodyData(null, options);

      //ASSERT
      Assert.AreEqual("\r\nSource Workspace: My Test workspace - 1111111", returnedString);
    }

    [Test]
    public void GetEmailBodyData_NoWorkspace_CorrectlyFormatedOutput()
    {
      int workspaceId = 1111111;
      var helper = NSubstitute.Substitute.For<IHelper>();
      Workspace workspace = null;

      IEmailBodyData mockDocumentTransferProvider = new mockDocumentTransferProvider(helper, workspace);

      var settings = new DocumentTransferSettings { SourceWorkspaceArtifactId = workspaceId };
      var options = JsonConvert.SerializeObject(settings);

      //ACT
      var returnedString = mockDocumentTransferProvider.GetEmailBodyData(null, options);

      //ASSERT
      Assert.AreEqual("", returnedString);
    }
    #endregion Read
  }

  internal class mockDocumentTransferProvider : DocumentTransferProvider.DocumentTransferProvider
  {
    private Workspace _workspace;
    public mockDocumentTransferProvider(IHelper helper, Workspace workspace) : base(helper)
    {
      _workspace = workspace;
    }

    protected override Workspace GetWorkspace(int workspaceArtifactIds)
    {
      return _workspace;
    }
  }
}