using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core.Api.Shared.Manager.Export;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Services.Export
{
	[TestFixture]
	public class RelativityExporterServiceTests
	{
		private IExporter exporter;

		[TestFixtureSetUp]
		public void Setup()
		{
			exporter = Substitute.For<IExporter>();
		}

		[Test]
		public void GoldFlow()
		{
			// Arrange
			object[] obj = new[] {new object[] { "REL01", "FileName", 1111 }};
			int[] avfIds = new[] {1, 2};
			int[] fieldArtifactIds = new[] {123, 456};
			global::Relativity.Core.Export.InitializationResults result = new global::Relativity.Core.Export.InitializationResults();
			result.RunId = Guid.NewGuid();
			result.ColumnNames = new[] {"Control Num", "FileName"};

			exporter.InitializeExport(0, null, 0).Returns(result);
			exporter.RetrieveResults(result.RunId, avfIds, 1).Returns(obj);

			ArtifactDTO expecteDto = new ArtifactDTO(1111, 10, new []
			{
				new ArtifactFieldDTO()
				{
					ArtifactId = 123,
					Value = "REL01",
					Name = "Control Num"
				},
				new ArtifactFieldDTO()
				{
					ArtifactId = 456,
					Value = "FileName",
					Name = "FileName"
				},
			});

			// Act
			RelativityExporterService rel = new RelativityExporterService(exporter, avfIds, fieldArtifactIds);
			ArtifactDTO[] data = rel.RetrieveData(1);


			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(1, data.Length);
			ArtifactDTO artifact = data[0];

			ValidateArtifact(expecteDto, artifact);
		}


		[Test]
		public void RetrieveNoData()
		{
			// Arrange
			int[] avfIds = new[] { 1, 2 };
			int[] fieldArtifactIds = new[] { 123, 456 };
			global::Relativity.Core.Export.InitializationResults result = new global::Relativity.Core.Export.InitializationResults();
			result.RunId = Guid.NewGuid();
			result.ColumnNames = new[] { "Control Num", "FileName" };

			exporter.InitializeExport(0, null, 0).Returns(result);
			exporter.RetrieveResults(result.RunId, avfIds, 1).Returns((object)null);

			// Act
			RelativityExporterService rel = new RelativityExporterService(exporter, avfIds, fieldArtifactIds);
			ArtifactDTO[] data = rel.RetrieveData(1);


			// Assert
			Assert.NotNull(data);
			Assert.AreEqual(0, data.Length);
		}

		private void ValidateArtifact(ArtifactDTO expect, ArtifactDTO actual)
		{
			Assert.AreEqual(expect.ArtifactId, actual.ArtifactId);
			Assert.AreEqual(expect.ArtifactTypeId, expect.ArtifactTypeId);
			for (int i = 0; i < expect.Fields.Count; i++)
			{
				ArtifactFieldDTO expectedField = expect.Fields[i];
				ArtifactFieldDTO actualField = actual.Fields[i];

				Assert.AreEqual(expectedField.Name, actualField.Name);
				Assert.AreEqual(expectedField.Value, actualField.Value);
				Assert.AreEqual(expectedField.ArtifactId, actualField.ArtifactId);
				Assert.AreEqual(expectedField.FieldType, actualField.FieldType);
			}
		}
	}
}