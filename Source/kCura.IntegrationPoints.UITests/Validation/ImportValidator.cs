using System;
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
		private IList<RelativityObject> _custodians;
		private readonly IRSAPIService _service;

		public ImportValidator(IRSAPIService service)
		{
			_service = service;
		}

		public void ValidateCustodians(Dictionary<string, string> expectedCustodians)
		{
			LoadCustodians();
			CompareCustodians(expectedCustodians);
		}

		private void LoadCustodians()
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = Guid.Parse(ObjectTypeGuids.Custodian)
				},
				Fields = new[]
				{
					new FieldRef { Name = "FullName" },
					new FieldRef { Name = "Manager" }
				},
			};
			_custodians = _service.RelativityObjectManager.Query(request);
		}

		private void CompareCustodians(Dictionary<string, string> expectedCustodians)
		{
			foreach (var expectedCustodian in expectedCustodians)
			{
				string fullName = expectedCustodian.Key;
				string expectedManagerName = expectedCustodian.Value;
				RelativityObject custodian = _custodians.FirstOrDefault(c => fullName.Equals(c["FullName"].Value));
				Assert.IsNotNull(custodian);
				var actualManager = custodian["manager"].Value as RelativityObjectValue;
				Assert.IsNotNull(actualManager);
				Assert.AreEqual(expectedManagerName, actualManager.Name);
			}
		}
	}
}
