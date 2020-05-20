﻿using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Actions;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.LDAPProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.WEB_IMPORT_EXPORT)]
	[Category(TestCategory.NOT_WORKING_ON_TRIDENT)] //Remove when REL-389924
	[Category(TestCategory.IMPORT_FROM_FTP_AND_LDAP)]
	[Category(TestCategory.NOT_WORKING_ON_REGRESSION_ENVIRONMENT)] // no access to LDAP from R1
	public class ImportLdapProviderTest : UiTest
	{
		private IRSAPIService _service;
		private IntegrationPointsImportLdapAction _integrationPointsAction;

		public ImportLdapProviderTest() : base(shouldImportDocuments: false)
		{ }

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			await SourceContext.AddEntityObjectToWorkspaceAsync().ConfigureAwait(false);

			Install(SourceContext.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();
		}

		[SetUp]
		public void SetUp()
		{
			_integrationPointsAction = new IntegrationPointsImportLdapAction(Driver, SourceContext);
		}

		[IdentifiedTest("ebfc56e6-5ac7-4694-9fde-7e474163f87e")]
		[RetryOnError]
		[Order(1)]
		public void DocumentExportToLoadFile_TC_IMPORT_CUST_1()
		{
			// Arrange
			var model = new ImportFromLdapModel("Import Entities", TransferredObjectConstants.ENTITY);

			// Step 1
			model.General.TransferredObject = TransferredObjectConstants.ENTITY;

			// Step 2
			model.Source.Authentication = LdapAuthenticationType.SecureSocketLayer;
			model.Source.ConnectionPath = SharedVariables.LdapConnectionPath;
			model.Source.ObjectFilterString = "(objectClass=organizationalPerson)";
			model.Source.ImportNestedItems = false;

			model.Source.Password = new SecureString();
			foreach (char c in SharedVariables.LdapPassword)
			{
				model.Source.Password.AppendChar(c);
			}

			model.Source.Username = new SecureString();
			foreach (char c in SharedVariables.LdapUsername)
			{
				model.Source.Username.AppendChar(c);
			}

			// Step 3
			model.ImportEntitySettingsModel.UniqueIdentifier = "UniqueID";
			model.ImportEntitySettingsModel.EntityManagerContainsLink = true;

			model.SharedImportSettings.Overwrite = OverwriteType.AppendOverlay;
			model.SharedImportSettings.FieldMapping = new List<Tuple<string, string>>();
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("objectguid", "UniqueID"));
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("givenname", "First Name"));
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("sn", "Last Name"));
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("manager", "Manager"));

			var validator = new ImportValidator(_service);

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewImportLdapIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			var expectedEntities = new Dictionary<string, string>
			{
				{"Wolny, Stan", "Kukla, Krzysztof"}
			};

			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted, JobStatusChoices.JobHistoryCompletedWithErrors);
			validator.ValidateEntities(expectedEntities);
		}
	}
}
