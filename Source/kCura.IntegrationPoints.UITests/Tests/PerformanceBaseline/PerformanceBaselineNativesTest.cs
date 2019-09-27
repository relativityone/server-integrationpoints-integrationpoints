using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.PerformanceBaseline
{
	[TestFixture, Explicit]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.PERFORMANCE_BASELINE)]
	public class PerformanceBaselineNativesTest : RelativityProviderTestsBase

	{
		private const string _WORKSPACE_NAME = "[Do Not Delete] perf60 with dg";

		private static readonly string[] _fieldsToMap =
		{
			"Control Number",
			"Alert",
			"All Custodians",
			"All Custodians (Script)",
			"All Paths/Locations",
			"All Source Locations (Script)",
			"Attachment Document IDs",
			"Attachment List",
			"Attorney Review Comments",
			"Author",
			"Bates Beg",
			"Bates Beg Attach",
			"Bates End",
			"Bates End Attach",
			"CC (SMTP Address)",
			"Child MD5 Hash Values",
			"Child SHA1 Hash Values",
			"Child SHA256 Hash Values",
			"Classification Index",
			"Comments",
			"Company",
			"Conceptual Index",
			"Confidential",
			"Container extension",
			"Container ID",
			"Container name",
			"Contains Embedded Files",
			"Control Number Beg Attach",
			"Control Number End Attach",
			"Conversation",
			"Conversation Family",
			"Conversation Index",
			"Created Date",
			"Created Date/Time",
			"Created Time",
			"Custodian",
			"Document Folder Path",
			"Document Subject",
			"Document Title",
			"Domains (Email BCC)",
			"Domains (Email CC)",
			"Domains (Email From)",
			"Domains (Email To)",
			"Email Action",
			"Email Author Date ID",
			"Email BCC",
			"Email Categories",
			"Email CC",
			"Email Created Date/Time",
			"Email Duplicate ID",
			"Email Duplicate Spare",
			"Email Entry ID",
			"Email Folder Path",
			"Email Format",
			"Email From",
			"Email From (SMTP Address)",
			"Email Has Attachments",
			"Email In Reply To ID",
			"Email Last Modified Date/Time",
			"Email Modified Flag",
			"Email Read Receipt Requested",
			"Email Received Date",
			"Email Received Date/Time",
			"Email Received Time",
			"Email Recipient Count",
			"Email Recipient Domains (BCC)",
			"Email Recipient Domains (CC)",
			"Email Recipient Domains (To)",
			"Email Recipient Name (To)",
			"Email Sensitivity",
			"Email Sent Date",
			"Email Sent Flag",
			"Email Store Name",
			"Email Subject",
			"Email Thread Group",
			"Email Thread Hash",
			"Email Threading Display",
			"Email Threading ID",
			"Email To",
			"Email To (SMTP Address)",
			"Email Unread",
			"Excel Hidden Columns",
			"Excel Hidden Rows",
			"Excel Hidden Worksheets",
			"Excel Pivot Tables",
			"Extracted Text",
			"Extracted Text Size in KB",
			"Family Group",
			"File Extension",
			"File Name",
			"File Size",
			"File Type",
			"Has Hidden Data",
			"Has OCR Text",
			"Is Parent",
			"Issues",
			"Keywords",
			"Title",
			"Sender Name"
		};
		private static readonly List<Tuple<string, string>> _fieldsMappings = CreateIdentityFieldMapping(_fieldsToMap);

		private static List<Tuple<string, string>> CreateIdentityFieldMapping(IEnumerable<string> fields)
		{
			return fields
				.Select(field => new Tuple<string, string>(field, field))
				.ToList();
		}


		[IdentifiedTest("db711d7a-6de6-4341-9183-11f71b0b88c1")]
		[RetryOnError]
		public void PerfromanceBaseline_10k_Copy_Natives_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "10k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 10k");
		}

		[IdentifiedTest("67651666-3d47-47af-826e-ea898055177f")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public void PerfromanceBaseline_10k_Copy_Natives_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "10k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 10k");
		}

		[IdentifiedTest("4be57e31-13cb-4aac-8af5-641c7eb0a6f0")]
		[RetryOnError]
		public void PerfromanceBaseline_10k_Links_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "10k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 10k");
		}

		[IdentifiedTest("18b7801f-e051-4aa5-a2d7-f8ff50eeb66b")]
		[RetryOnError]
		public void PerfromanceBaseline_10k_Links_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "10k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 10k");
		}

		[IdentifiedTest("2d87faf4-5fd2-4acd-9360-b496dc7db348")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Copy_Natives_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "100k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 100k");
		}

		[IdentifiedTest("c5ab77da-dda0-4347-afb4-9e48814bd794")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Copy_Natives_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "100k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 100k");
		}

		[IdentifiedTest("4e78faf8-babf-49bb-91d4-c9c9b8a33b96")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Links_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "100k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 100k");
		}

		[IdentifiedTest("18d78559-a856-4c28-835b-7a6ce68532f7")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Links_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "100k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 100k");
		}

		[IdentifiedTest("71f8bcf3-d36b-4d8a-9d64-b413bf82fe27")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Metadata_Only_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "100k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 100k");
		}

		[IdentifiedTest("13787f9a-5f71-4524-aeb3-37faa8768869")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Metadata_Only_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "100k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 100k");
		}

		[IdentifiedTest("40b86b04-f1a9-45e8-b027-a025a3f31cbf")]
		[RetryOnError]
		public void PerfromanceBaseline_500k_Copy_Natives_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "500k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 500k");
		}

		[IdentifiedTest("8b0e6547-80ea-4338-a1d1-b6021ed7e460")]
		[RetryOnError]
		public void PerfromanceBaseline_500k_Copy_Natives_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "500k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 500k");
		}

		[IdentifiedTest("93671575-82d8-4010-9fc6-0d6c162dc6ac")]
		[RetryOnError]
		public void PerfromanceBaseline_500k_Links_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "500k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 500k");
		}

		[IdentifiedTest("591c7287-9201-429e-b2a7-98f93a427331")]
		[RetryOnError]
		public void PerfromanceBaseline_500k_Links_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "500k";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: 500k");
		}

		[IdentifiedTest("fdcb0c4d-a249-4ace-916b-635accb89d9f")]
		[RetryOnError]
		public void PerfromanceBaseline_1000k_Copy_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "All documents";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: All documents");
		}

		[IdentifiedTest("59bd443d-0724-4622-b4e8-df5f9bb77139")]
		[RetryOnError]
		public void PerfromanceBaseline_1000k_Copy_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "All documents";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: All documents");
		}

		[IdentifiedTest("6729ec9e-0f97-4230-9f0d-8a2c5b0376a2")]
		[RetryOnError]
		public void PerfromanceBaseline_1000k_Links_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "All documents";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: All documents");
		}

		[IdentifiedTest("b2bc3250-a585-435a-aa8d-df9fccb8fa01")]
		[RetryOnError]
		public void PerfromanceBaseline_1000k_Links_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();

			model.SavedSearch = "All documents";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			generalProperties.Properties["Source Details:"].Should().Be("Saved Search: All documents");
		}

		private RelativityProviderModel CreateRelativityProviderModel()
		{
			SourceContext.WorkspaceName = _WORKSPACE_NAME;

			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name);
			{
				model.Source = RelativityProviderModel.SourceTypeEnum.SavedSearch;
				model.RelativityInstance = "This Instance";
				model.DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}";
				model.FieldMapping = _fieldsMappings;
			}

			return model;
		}
	}
}
