using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using Relativity.Services.ArtifactGuid;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Services.Interfaces.ObjectType;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	public class ChoiceServiceTests : SourceProviderTemplate
	{
		private IChoiceService _sut;

		public ChoiceServiceTests() : base("ChoiceServiceTests Workspace")
		{
		}

		[SetUp]
		public void SetUp()
		{
			_sut = Container.Resolve<IChoiceService>();
		}

		[Test]
		public async Task GetChoicesOnField_ShouldReturnExpectedChoiceValues_WhenChoiceArtifactIdIsPassed()
		{
			// Arrange
			int jobHistoryRdoTypeId = await GetRdoArtifactTypeId(ObjectTypeGuids.JobHistoryErrorGuid).ConfigureAwait(false);

			List<FieldEntry> expectedChoiceFields = new List<FieldEntry>
			{
				new FieldEntry {DisplayName = "Error Status", IsRequired = false},
				new FieldEntry {DisplayName = "Error Type", IsRequired = false}
			};

			// Act
			List<FieldEntry> result = _sut.GetChoiceFields(WorkspaceArtifactId, jobHistoryRdoTypeId);
			
			// Assert
			result.ShouldAllBeEquivalentTo(expectedChoiceFields, config => config.Excluding(x => x.FieldIdentifier));
		}

		private async Task<int> GetRdoArtifactTypeId(Guid rdoGuid)
		{
			using (IArtifactGuidManager guidManager = Helper.CreateProxy<IArtifactGuidManager>())
			using (IObjectTypeManager objectTypeManager = Helper.CreateProxy<IObjectTypeManager>())
			{
				int jobHistoryErrorTypeId = await guidManager.ReadSingleArtifactIdAsync(WorkspaceArtifactId, rdoGuid)
					.ConfigureAwait(false);

				var jobHistoryErrorType = await objectTypeManager.ReadAsync(WorkspaceArtifactId, jobHistoryErrorTypeId).ConfigureAwait(false);

				return jobHistoryErrorType.ArtifactTypeID;
			}
		}
	}
}
