﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Data.RowDataGateway;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Export
{
	public class DynamicFolderPathReaderTests : TestBase
	{
		private BaseContext _dbContext;
		private DynamicFolderPathReader _instance;

		public override void SetUp()
		{
			_dbContext = Substitute.For<BaseContext>();
			_instance = new DynamicFolderPathReader(_dbContext);
		}

		[Test]
		public void ItShouldSetFolderPath()
		{
			// Arrange
			var paths = new Dictionary<int, string>
			{
				{266672, "path1"},
				{764659, "path2"}
			};

			DataTable dataTable = CreateDataTable(paths);

			List<ArtifactDTO> artifactDtos = paths.Select(x => new ArtifactDTO(x.Key, 1, string.Empty, new List<ArtifactFieldDTO>())).ToList();

			_dbContext.ExecuteSqlStatementAsDataTable(Arg.Any<string>()).Returns(dataTable);

			// Act
			_instance.SetFolderPaths(artifactDtos);

			// Assert
			foreach (var path in paths)
			{
				var artifactDto = artifactDtos.First(x => x.ArtifactId == path.Key);
				var actualPath = artifactDto.GetFieldByName(IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME).Value;
				Assert.That(actualPath, Is.EqualTo(path.Value));
			}
		}

		[Test]
		public void ItShouldHandleEmptyList()
		{
			_instance.SetFolderPaths(new List<ArtifactDTO>());

			Assert.Pass();
		}

		private DataTable CreateDataTable(IDictionary<int, string> paths)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ArtifactId", typeof(int));
			dataTable.Columns.Add("FolderPath", typeof(string));

			foreach (var path in paths)
			{
				var dataRow = dataTable.NewRow();
				dataRow["ArtifactId"] = path.Key;
				dataRow["FolderPath"] = path.Value;
				dataTable.Rows.Add(dataRow);
			}
			return dataTable;
		}
	}
}