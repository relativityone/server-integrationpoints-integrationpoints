﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointTypeManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class IntegrationPointTypeManagerTests : SourceProviderTemplate
	{
		public IntegrationPointTypeManagerTests() : base($"IPType_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointTypeManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateProxy<IIntegrationPointTypeManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client?.Dispose();
		}

		[IdentifiedTest("0a6b1938-ae47-4420-9019-1a203ecb47b4")]
		public void ItShouldRetrieveAllIntegrationPointTypes()
		{
			var expected = GetAllTypes();

			var actual = _client.GetIntegrationPointTypes(WorkspaceArtifactId).Result;

			Assert.That(actual, Is.EquivalentTo(expected.Keys).Using(new Func<IntegrationPointTypeModel, string, bool>((x, y) => (x.Name == y) && (x.ArtifactId == expected[y]))));
		}

		private IDictionary<string, int> GetAllTypes()
		{
			string sqlStatement = "SELECT [ArtifactID], [Name] FROM [IntegrationPointType]";
			var providersDataTable = Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsDataTable(sqlStatement);

			IDictionary<string, int> providers = new Dictionary<string, int>();
			foreach (DataRow dataRow in providersDataTable.Rows)
			{
				providers.Add(dataRow["Name"].ToString(), int.Parse(dataRow["ArtifactID"].ToString()));
			}
			return providers;
		}
	}
}