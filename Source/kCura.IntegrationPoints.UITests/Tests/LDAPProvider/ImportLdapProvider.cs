﻿

using System;
using System.Collections.Generic;
using System.Security;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.ExportToLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Ldap;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Actions;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using kCura.Vendor.Castle.Core.Internal;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.LDAPProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class ImportLdapProvider : UiTest

	{
		private IntegrationPointsImportLdapAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsImportLdapAction(Driver, Context);
		}


		[Test, Order(1)]
		public void DocumentExportToLoadFile_TC_IMPORT_CUST_1()
		{
			// Arrange

			var model = new ImportFromLdapModel("Import Custodians", ExportToLoadFileTransferredObjectConstants.CUSTODIAN);

			// Step 1

			model.General.TransferredObject = ExportToLoadFileTransferredObjectConstants.CUSTODIAN;


			// Step 2

			model.Source.Authentication = LdapAuthenticationType.SecureSocketLayer;
			model.Source.ConnectionPath = SharedVariables.LdapConnectionPath;
			model.Source.ImportNestedItems = false;
			model.Source.ObjectFilterString = "";
			model.Source.Password = new SecureString();
			SharedVariables.LdapPassword.ForEach(c => model.Source.Password.AppendChar(c));

			model.Source.Username = new SecureString();
			SharedVariables.LdapUsername.ForEach(c => model.Source.Username.AppendChar(c));

			// Step 3

			model.ImportCustodianSettingsModel.UniqueIdentifier = "UniqueID";
			model.ImportCustodianSettingsModel.CustodianManagerContainsLink = true;

			model.SharedImportSettings.Overwrite = OverwriteType.AppendOverlay;
			model.SharedImportSettings.FieldMapping = new List<Tuple<string, string>>();
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("objectguid", "UniqueID"));
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("givenname", "First Name"));
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("sn", "Last Name"));
			model.SharedImportSettings.FieldMapping.Add(new Tuple<string, string>("manager", "Manager"));

			//_integrationPointsAction.


			//var validator = new ExportToLoadFileProviderValidator();

			//// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewImportLdapIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			//detailsPage.RunIntegrationPoint();

			//// Assert
			//validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
		}
	}
}
