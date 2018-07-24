using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using ObjectTypeGuids = kCura.IntegrationPoints.Core.Contracts.Custodian.ObjectTypeGuids;

namespace kCura.IntegrationPoints.UITests.Validation
{
	public class ImportValidator : BaseUiValidator
	{
		private IList<RelativityObject> _entities;
		private readonly IRSAPIService _service;

		public ImportValidator(IRSAPIService service)
		{
			_service = service;
		}

		public void ValidateEntities(Dictionary<string, string> expectedEntities)
		{
			LoadEntities();
			CompareEntities(expectedEntities);
		}

		private void LoadEntities()
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = ObjectTypeGuids.Entity
				},
				Fields = new[]
				{
					new FieldRef { Name = "FullName" },
					new FieldRef { Name = "Manager" }
				},
			};
			_entities = _service.RelativityObjectManager.Query(request);
		}

		private void CompareEntities(Dictionary<string, string> expectedEntities)
		{
			foreach (var expectedEntity in expectedEntities)
			{
				string fullName = expectedEntity.Key;
				string expectedManagerName = expectedEntity.Value;
				RelativityObject entity = _entities.FirstOrDefault(c => fullName.Equals(c["FullName"].Value));
				Assert.IsNotNull(entity);
				var actualManager = entity["manager"].Value as RelativityObjectValue;
				Assert.IsNotNull(actualManager);
				Assert.AreEqual(expectedManagerName, actualManager.Name);
			}
		}
	}
}
