using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public class DynamicFolderPathReader : IFolderPathReader
    {
        private const string _DYNAMIC_FOLDER_PATH_SQL = @"
            DECLARE @FolderPaths TABLE (ArtifactId INT, FolderPath NVARCHAR(MAX));
            INSERT @FolderPaths(ArtifactId) (SELECT * FROM @ArtifactIds);

            DECLARE @ArtifactId INT;

            DECLARE @path NVARCHAR(MAX);
            DECLARE @docID INT;
            DECLARE @folderID INT;
            DECLARE @current INT;

            DECLARE @delimiter NVARCHAR(3) SET @delimiter = CAST('\' AS NVARCHAR(1))
            DECLARE @rootFolderID INT SET @rootFolderID = (SELECT ArtifactID FROM SystemArtifact WITH (NOLOCK) WHERE SystemArtifactIdentifier = 'RootFolder')

            DECLARE MY_CURSOR CURSOR
              LOCAL STATIC READ_ONLY FORWARD_ONLY
            FOR
            SELECT ArtifactId FROM @FolderPaths

            OPEN MY_CURSOR
            FETCH NEXT FROM MY_CURSOR INTO @ArtifactId
            WHILE @@FETCH_STATUS = 0
            BEGIN
                --BEGIN RETRIEVING PATH
                SET @path = NULL
                SET @docID = @ArtifactId

                SELECT TOP 1 @docID = ArtifactID, @folderID = ParentArtifactID_D FROM Document WITH (NOLOCK) WHERE ArtifactId = @docID

                SET @current = @docID

                WHILE NOT (SELECT TOP 1 ParentArtifactID FROM Artifact WITH (NOLOCK) WHERE ArtifactID = @current) IS NULL BEGIN
                    IF NOT @path IS NULL AND NOT @current = @rootFolderID BEGIN
                        SET @path = (SELECT @delimiter + @path)
                    END
                    SET @current = (SELECT ParentArtifactID FROM Artifact WITH (NOLOCK) WHERE ArtifactID = @current)
                    IF NOT @current = @docID AND NOT @current = @rootFolderID BEGIN
                        SET @path = (SELECT TextIdentifier FROM Artifact WITH (NOLOCK) WHERE ArtifactID = @current) + ISNULL(@path, '')
                    END
                END

                UPDATE @FolderPaths SET FolderPath = @path WHERE ArtifactId = @ArtifactId

                --END
                FETCH NEXT FROM MY_CURSOR INTO @ArtifactId
            END
            CLOSE MY_CURSOR
            DEALLOCATE MY_CURSOR

            SELECT * FROM @FolderPaths";
        private readonly IWorkspaceDBContext _dbContext;

        public DynamicFolderPathReader(IWorkspaceDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void SetFolderPaths(List<ArtifactDTO> artifacts)
        {
            if (artifacts.Count == 0)
            {
                return;
            }
            var artifactsDict = artifacts.ToDictionary(x => x.ArtifactId, x => x);
            SetFolderPaths(artifactsDict);
        }

        private void SetFolderPaths(IDictionary<int, ArtifactDTO> artifacts)
        {
            DataTable artifactIdsValues = artifacts.Keys.ToDataTable();

            SqlParameter parameter = new SqlParameter
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "IDs",
                Value = artifactIdsValues,
                ParameterName = "@ArtifactIds"
            };

            var dataTable = _dbContext.ExecuteSqlStatementAsDataTable(_DYNAMIC_FOLDER_PATH_SQL, new[]{parameter});

            foreach (DataRow dataTableRow in dataTable.Rows)
            {
                int artifactId = (int) dataTableRow["ArtifactId"];
                string path = dataTableRow["FolderPath"].ToString();
                artifacts[artifactId].Fields.Add(new ArtifactFieldDTO
                {
                    Name = IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME,
                    Value = path
                });
            }
        }
    }
}
