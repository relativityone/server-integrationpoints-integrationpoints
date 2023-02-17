using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity;
using Relativity.Kepler.Transport;
using Relativity.Services.Interfaces.Document;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class DocumentService
    {
        private static ITestHelper _testHelper = null;
        private static readonly string[] AllFields = new[] { "*" };

        private static ITestHelper Helper
        {
            get
            {
                if (_testHelper == null)
                {
                    _testHelper = new TestHelper();
                }
                return _testHelper;
            }
        }

        public static List<Document> GetAllDocuments(int workspaceId, string[] requestedFields)
        {
            using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType    = new ObjectTypeRef {ArtifactTypeID = (int)ArtifactType.Document },
                    Fields = requestedFields.Select(x => new FieldRef {Name = x})
                };

                return objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue)
                    .GetAwaiter().GetResult().Objects.Select(x => new Document(x)).ToList();
            }
        }

        public static List<Document> GetAllDocuments(int workspaceId)
        {
            return GetAllDocuments(workspaceId, AllFields);
        }

        public static void DeleteAllDocuments(int workspaceId)
        {
            using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
            {
                MassDeleteByCriteriaRequest request = new MassDeleteByCriteriaRequest
                {
                    ObjectIdentificationCriteria = new ObjectIdentificationCriteria
                    {
                        ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Document}
                    }
                };

                objectManager.DeleteAsync(workspaceId, request).GetAwaiter().GetResult();
            }
        }

        public static string GetNativeMD5String(int workspaceId, Document document)
        {
            using (IDocumentFileManager documentFileManager = Helper.CreateProxy<IDocumentFileManager>())
            {
                IKeplerStream nativeFile = documentFileManager
                    .DownloadNativeFileAsync(workspaceId, document.ArtifactId)
                    .GetAwaiter().GetResult();

                Stream nativeStream = nativeFile.GetStreamAsync().GetAwaiter().GetResult();

                return System.BitConverter.ToString(MD5.Create().ComputeHash(nativeStream));
            }
        }
    }
}
