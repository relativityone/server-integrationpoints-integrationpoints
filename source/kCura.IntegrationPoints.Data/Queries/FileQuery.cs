using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class FileQuery : IFileQuery
    {
        private IWorkspaceDBContext _workspaceDbContext;
        public FileQuery(IWorkspaceDBContext workspaceDbContext)
        {
            _workspaceDbContext = workspaceDbContext;
        }

        public IEnumerable<FileInfo> GetDocumentFiles(string documentArtifactIDs, int fileType)
        {
            var sql = string.Format("SELECT * FROM [File] WHERE Type={0} AND DocumentArtifactID IN ({1})", fileType, documentArtifactIDs);

            var dt = _workspaceDbContext.ExecuteSqlStatementAsDataTable(sql);

            var nativeFilePaths = new List<FileInfo>();

            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    nativeFilePaths.Add(new FileInfo { Location = row.Field<string>("Location"), Filename = row.Field<string>("Filename") } );
                }
            }

            return nativeFilePaths;
        }
    }
}
