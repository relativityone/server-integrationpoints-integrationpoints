using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.JobHistory;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.JobHistory
{
	public class CompletedJobQueryBuilderTests : TestBase
	{
		private CompletedJobQueryBuilder _builder;


		public override void SetUp()
		{
			_builder = new CompletedJobQueryBuilder();
		}

		[Test]
		public void ItShouldCreateValidQuery()
		{
			var query = _builder.CreateQuery(string.Empty, false, new List<int>());

			Assert.That(query.ArtifactTypeGuid.Value.ToString().ToUpperInvariant(), Is.EqualTo(ObjectTypeGuids.JobHistory.ToUpperInvariant()));
			Assert.That(query.Fields[0].Name, Is.EqualTo(FieldValue.AllFields[0].Name));

			var condition = query.Condition as SingleChoiceCondition;
			Assert.That(condition, Is.Not.Null);
			Assert.That(condition.Operator, Is.EqualTo(SingleChoiceConditionEnum.AnyOfThese));
			Assert.That(condition.Guid.ToString().ToUpperInvariant(), Is.EqualTo(JobHistoryFieldGuids.JobStatus.ToUpperInvariant()));

			var statusGuids = new List<Guid> {JobStatusChoices.JobHistoryCompleted.Guids[0], JobStatusChoices.JobHistoryCompletedWithErrors.Guids[0]};
			Assert.That(condition.Value as List<Guid>, Is.EquivalentTo(statusGuids));
		}

		[Test]
		[TestCase(true, SortEnum.Descending)]
		[TestCase(false, SortEnum.Ascending)]
		public void ItShouldSetSortAccordingly(bool sortDescending, SortEnum expectedSort)
		{
			var query = _builder.CreateQuery(string.Empty, sortDescending, new List<int>());

			Assert.That(query.Sorts[0].Direction, Is.EqualTo(expectedSort));
		}

		[Test]
		[TestCase("column_name", "column_name")]
		[TestCase("", nameof(JobHistoryModel.DestinationWorkspace))]
		[TestCase(null, nameof(JobHistoryModel.DestinationWorkspace))]
		public void ItShouldSetSortColumnAccordingly(string sortColumn, string expectedSortColumn)
		{
			var query = _builder.CreateQuery(sortColumn, false, new List<int>());

			Assert.That(query.Sorts[0].Field, Is.EqualTo(expectedSortColumn));
		}
	}
}