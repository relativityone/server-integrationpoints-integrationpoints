using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class DocumentService
	{
		private static ITestHelper _testHelper = null;
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

		public static List<Result<Document>> GetAllDocuments(int workspaceId, string[] requestedFields)
		{
			using (IRSAPIClient proxy = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				proxy.APIOptions.WorkspaceID = workspaceId;

				List<FieldValue> fields = requestedFields.Select(x => new FieldValue(x)).ToList();
				Query<Document> query = new Query<Document>
				{
					Fields = fields
				};

				QueryResultSet<Document> result = null;
				result = proxy.Repositories.Document.Query(query, 0);
				return result.Results;
			}
		}

		public static void DeleteAllDocuments(int workspaceId)
		{
			using (IRSAPIClient proxy = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				proxy.APIOptions.WorkspaceID = workspaceId;

				Query<Document> query = new Query<Document>
				{
					Fields = new List<FieldValue> { new FieldValue("Control Number") }
				};
				  
				QueryResultSet<Document> result = null;
				result = proxy.Repositories.Document.Query(query, 0);
				proxy.Repositories.Document.Delete(result.Results.Select(x => x.Artifact).ToList());
			}
		}

		public static string GetNativeMD5String(int workspaceId, Result<Document> docResult)
		{
			string result = string.Empty;

			using (IRSAPIClient proxy = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				proxy.APIOptions.WorkspaceID = workspaceId;

				KeyValuePair<DownloadResponse, Stream> nativeResponse = proxy.Repositories.Document.DownloadNative(docResult.Artifact);

				if (nativeResponse.Key != null && nativeResponse.Value != null)
				{
					nativeResponse.Value.Seek(0, SeekOrigin.Begin);
					result = System.BitConverter.ToString(MD5.Create().ComputeHash(nativeResponse.Value));
				}
			}

			return result;
		}
	}
}
