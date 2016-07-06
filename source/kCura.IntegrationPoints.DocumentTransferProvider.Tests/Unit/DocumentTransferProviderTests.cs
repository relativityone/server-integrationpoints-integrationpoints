using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.DocumentTransferProvider;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;
using NSubstitute;
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
      var repositoryFactory = NSubstitute.Substitute.For<IRepositoryFactory>();
      var workspaceRepository = NSubstitute.Substitute.For<Repositories.IWorkspaceRepository>();
      WorkspaceDTO workspace = new WorkspaceDTO() { ArtifactId = workspaceId, Name = "My Test workspace" };
      repositoryFactory.GetWorkspaceRepository().Returns(workspaceRepository);
      workspaceRepository.Retrieve(Arg.Any<int>()).Returns(workspace);


      IEmailBodyData mockDocumentTransferProvider = new DocumentTransferProvider.DocumentTransferProvider(helper, repositoryFactory);

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
      var repositoryFactory = NSubstitute.Substitute.For<IRepositoryFactory>();
      var workspaceRepository = NSubstitute.Substitute.For<Repositories.IWorkspaceRepository>();
      WorkspaceDTO workspace = null;
      repositoryFactory.GetWorkspaceRepository().Returns(workspaceRepository);
      workspaceRepository.Retrieve(Arg.Any<int>()).Returns(workspace);

      IEmailBodyData mockDocumentTransferProvider = new DocumentTransferProvider.DocumentTransferProvider(helper, repositoryFactory);

      var settings = new DocumentTransferSettings { SourceWorkspaceArtifactId = workspaceId };
      var options = JsonConvert.SerializeObject(settings);

      //ACT
      var returnedString = mockDocumentTransferProvider.GetEmailBodyData(null, options);

      //ASSERT
      Assert.AreEqual("", returnedString);
    }
    #endregion Read
  }
}