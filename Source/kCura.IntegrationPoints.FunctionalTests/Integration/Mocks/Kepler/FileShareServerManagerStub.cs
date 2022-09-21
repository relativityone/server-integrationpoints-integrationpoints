using System.Collections.Generic;
using System.IO;
using Moq;
using Relativity.Services;
using Relativity.Services.ResourceServer;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class FileShareServerManagerStub : KeplerStubBase<IFileShareServerManager>
    {
        public void SetupFileShareServerManagerStub()
        {
            FileShareQueryResultSet fileShareQueryResultSet = new FileShareQueryResultSet
            {
                Results = new List<Result<FileShareResourceServer>>
                {
                    new Result<FileShareResourceServer>
                    {
                        Artifact = new FileShareResourceServer
                        {
                            UNCPath = Directory.GetCurrentDirectory()
                        }
                    }
                }
            };

            Mock.Setup(x => x.QueryAsync(It.IsAny<Relativity.Services.Query>())).ReturnsAsync(fileShareQueryResultSet);
        }
    }
}
