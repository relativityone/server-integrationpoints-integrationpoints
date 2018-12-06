using System;
using System.Collections.Generic;
using System.Security;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap;
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
	public class ImportLdapProviderTest : UiTest
	{
		private IRSAPIService _service;
		private IntegrationPointsImportLdapAction _integrationPointsAction;

		protected override bool InstallLegalHoldApp => true;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsImportLdapAction(Driver, Context);
			Install(Context.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();
		}

		[Test, Order(1)]
		[Category(TestCategory.SMOKE)]
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
			SharedVariables.LdapPassword.ForEach(c => model.Source.Password.AppendChar(c));

			model.Source.Username = new SecureString();
			SharedVariables.LdapUsername.ForEach(c => model.Source.Username.AppendChar(c));

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
				{"Szmigielski, Piotr", "Lorenz, Andrzej"}
			};

			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted, JobStatusChoices.JobHistoryCompletedWithErrors);
			validator.ValidateEntities(expectedEntities);
		}
	}
}
