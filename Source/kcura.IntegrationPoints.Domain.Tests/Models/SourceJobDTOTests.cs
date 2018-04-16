using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Domain.Tests.Models
{
	[TestFixture]
	public class SourceJobDTOTests
	{
		[TestCase]
		public void ItShouldReturn_CorrectObjectTypeRef()
		{
			int artifactTypeId = 543;
			var dto = new SourceJobDTO
			{
				ArtifactTypeId = artifactTypeId
			};

			var objectType = dto.ObjectTypeRef;

			Assert.AreEqual(artifactTypeId, objectType.ArtifactTypeID);
		}

		[TestCase]
		public void ItShouldReturn_CorrectParentObject()
		{
			int sourceWorkspaceArtifactId = 543;
			var dto = new SourceJobDTO
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId
			};

			var parentObject = dto.ParentObject;

			Assert.AreEqual(sourceWorkspaceArtifactId, parentObject.ArtifactID);
		}

		[TestCase]
		public void ItShouldReturn_CorrectFieldRefValuePairs()
		{
			int jobHistoryArtifactId = 56543;
			string jobHistoryName = "Job History - Name";
			string name = "name!";

			var dto = new SourceJobDTO
			{
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = jobHistoryName,
				Name = name
			};

			var fields = dto.FieldRefValuePairs;


			AssertValueExistInFieldsList(fields, Constants.SOURCEJOB_NAME_FIELD_NAME, name);
			AssertValueExistInFieldsList(fields, Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME, jobHistoryArtifactId);
			AssertValueExistInFieldsList(fields, Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME, jobHistoryName);
		}

		private void AssertValueExistInFieldsList(List<FieldRefValuePair> fields, string fieldName, object expectedValue)
		{
			var field = fields.Single(x => x.Field.Name == fieldName);
			Assert.AreEqual(expectedValue, field.Value);
		}
	}
}
