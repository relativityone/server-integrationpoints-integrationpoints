using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class ChoiceRepositoryTests : TestBase
	{
		private IChoiceQuery _choiceQuery;
		private ChoiceRepository _choiceRepository;

		public override void SetUp()
		{
			_choiceQuery = Substitute.For<IChoiceQuery>();

			_choiceRepository = new ChoiceRepository(_choiceQuery);
		}

		[Test]
		public void ItShouldRetrieveAllOverwriteFieldChoices()
		{
			var expectedChoices = new List<Choice>
			{
				new Choice(756)
				{
					Name = "name_653"
				},
				new Choice(897)
				{
					Name = "name_466"
				}
			};

			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).Returns(expectedChoices);

			var actualChoicesModels = _choiceRepository.GetOverwriteFieldChoices();

			Assert.That(actualChoicesModels,
				Is.EquivalentTo(expectedChoices).Using(new Func<OverwriteFieldsModel, Choice, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactID))));
		}
	}
}