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
		private List<RelativityObject> _custodians;
		private readonly IRSAPIService _service;

		public ImportValidator(IRSAPIService service)
		{
			_service = service;
		}

		public void ValidateCustodians(Dictionary<string, bool> expectedCustodians)
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
					new FieldRef { Name = "Email" },
					new FieldRef { Name = "Manager" }
				},
			};
			_custodians = _service.RelativityObjectManager.Query(request);
		}

		private void CompareCustodians(Dictionary<string, bool> expectedCustodians)
		{
			foreach (var custodian in _custodians)
			{
				string email = (string) custodian["email"].Value;
				Assert.IsTrue(expectedCustodians.ContainsKey(email));
				bool shouldHaveManager = expectedCustodians[email];
			}
		}
	}
}
