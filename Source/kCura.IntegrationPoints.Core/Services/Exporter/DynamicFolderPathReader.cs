using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class DynamicFolderPathReader : IFolderPathReader
	{
		private const string _DYNAMIC_FOLDER_PATH_SQL = @"
declare @FolderPaths table (ArtifactId int, FolderPath nvarchar(max));
insert @FolderPaths(ArtifactId) values {0};

DECLARE @ArtifactId int;

DECLARE MY_CURSOR CURSOR 
  LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR 
SELECT DISTINCT ArtifactId 
FROM @FolderPaths

OPEN MY_CURSOR
FETCH NEXT FROM MY_CURSOR INTO @ArtifactId
WHILE @@FETCH_STATUS = 0
BEGIN
	--BEGIN RETRIEVING PATH
	DECLARE @path NVARCHAR(MAX);
	DECLARE @docID INT = @ArtifactId;
	DECLARE @folderID INT;
	DECLARE @current INT;

	DECLARE @delimiter NVARCHAR(3) SET @delimiter = CAST('\' AS NVARCHAR(1))
	DECLARE @rootFolderID INT SET @rootFolderID = (SELECT ArtifactID FROM SystemArtifact WITH (NOLOCK) WHERE SystemArtifactIdentifier = 'RootFolder')

	SET @path = NULL

	SELECT TOP 1 @docID = ArtifactID, @folderID = ParentArtifactID_D FROM [Document] WITH (NOLOCK) WHERE [ArtifactId] = @docID

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

		private readonly BaseContext _dbContext;

		public DynamicFolderPathReader(BaseContext dbContext)
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
			string artifactIdsValues = GetArtifactIdsValues(artifacts);

			var dataTable = _dbContext.ExecuteSqlStatementAsDataTable(string.Format(_DYNAMIC_FOLDER_PATH_SQL, artifactIdsValues));

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

		private string GetArtifactIdsValues(IDictionary<int, ArtifactDTO> artifacts)
		{
			return string.Join(",", artifacts.Select(x => $"({x.Key})"));
		}
	}
}