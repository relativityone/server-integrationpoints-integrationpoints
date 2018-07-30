using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using kCura.IntegrationPoints.Core.Contracts.Custodian;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{

	[TestFixture]
	public class CustodianExportToLoadFile : UiTest
	{
		private IntegrationPointsAction _integrationPointsAction;
		private IRSAPIService _service;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			EnsureGeneralPageIsOpened();
			Context.InstallLegalHold();
			Install(Context.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();

			SetupCustodians();

			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);

		}

		[Test]
		public void CustodianExportToLoadFile_TC_ELF_CUST_1()
		{
			CustodianExportToLoadFileModel model = new CustodianExportToLoadFileModel("TC-ELF-CUST-1");
			model.TransferredObject = "Custodian";
			model.ExportDetails = new CustodianExportToLoadFileDetails
			{
				View = "Custodians - Legal Hold View"
			};
			model.ExportDetails.SelectAllFields = true;
			model.ExportDetails.ExportTextFieldsAsFiles = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;

			model.OutputSettings = new ExportToLoadFileOutputSettingsModel
			{
				LoadFileOptions = new ExportToLoadFileLoadFileOptionsModel
				{
					DataFileFormat = "Comma-separated (.csv)",
					DataFileEncoding = "Unicode",
					ExportMultiChoiceAsNested = true,
					AppendOriginalFileName = true,
				},
				TextOptions = new ExportToLoadFileTextOptionsModel
				{
					TextFileEncoding = "Unicode (UTF-8)",
					TextPrecedence = "Notes",
					TextSubdirectoryPrefix = "TEXT",
				},
				VolumeAndSubdirectoryOptions = new ExportToLoadFileVolumeAndSubdirectoryModel
				{
					VolumePrefix = "VOL",
					VolumeStartNumber = 1,
					VolumeNumberOfDigits = 4,
					VolumeMaxSize = 4400,
					SubdirectoryStartNumber = 1,
					SubdirectoryNumberOfDigits = 4,
					SubdirectoryMaxFiles = 500
				}
			};
			var validator = new ExportToLoadFileProviderValidator();

			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportCustodianToLoadfileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateTransferedItems(detailsPage, 1);
		}

		private void SetupCustodians()
		{
			_service.RelativityObjectManager.Create(new ObjectTypeRef()
			{
				Guid = Core.Contracts.Custodian.ObjectTypeGuids.Custodian
			},
				new List<FieldRefValuePair>()
				{
					new FieldRefValuePair()
					{
						Field = new FieldRef {Guid = Guid.Parse(CustodianFieldGuids.FirstName)},
						Value = "First name"
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef {Guid = Guid.Parse(CustodianFieldGuids.LastName)},
						Value = "Last name"
					}
				});
		}
	}
}
