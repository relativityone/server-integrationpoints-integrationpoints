using System.Collections.Generic;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Components;
using NUnit.Framework;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation
{
	public abstract class RelativityProviderValidatorBase : BaseUiValidator
	{
		public void ValidateSummaryPage(PropertiesTable propertiesTable, RelativityProviderModel model, TestContext sourceContext, TestContext destinationContext, bool expectErrors)
		{
			Dictionary<string, string> propertiesTableDictionary = propertiesTable.Properties;

			ValidateHasErrorsProperty(propertiesTableDictionary, expectErrors);
			ValidateGeneralModel(propertiesTableDictionary, model, sourceContext, destinationContext);
		}

		protected virtual void ValidateGeneralModel(Dictionary<string, string> propertiesTableDictionary, RelativityProviderModel model, TestContext sourceContext, TestContext destinationContext)
		{
			Assert.AreEqual(model.GetValueOrDefault(x => x.Name), propertiesTableDictionary["Name:"]);
			StringAssert.AreEqualIgnoringCase(GetExpectedExportType(model), propertiesTableDictionary["Export Type:"]);
			StringAssert.AreEqualIgnoringCase(GetExpectedSourceDetails(model), propertiesTableDictionary["Source Details:"]);
			Assert.AreEqual(sourceContext.WorkspaceName, propertiesTableDictionary["Source Workspace:"]);
			StringAssert.AreEqualIgnoringCase(model.GetValueOrDefault(x => x.TransferredObject), propertiesTableDictionary["Transfered Object:"]);
			Assert.AreEqual(destinationContext.WorkspaceName, propertiesTableDictionary["Destination Workspace:"]);
			Assert.AreEqual(OverwriteModeEnumToString(model.GetValueOrDefault(x => x.Overwrite)), propertiesTableDictionary["Overwrite:"]);
			Assert.AreEqual(MultiSelectFieldOverlayBehaviorEnumToString(model.GetValueOrDefault(x => x.MultiSelectFieldOverlay)), propertiesTableDictionary["Multi-Select Overlay:"]);
			Assert.AreEqual(UseFolderPathInformationEnumToString(model.GetValueOrDefault(x => x.UseFolderPathInformation)), propertiesTableDictionary["Use Folder Path Info:"]);
			Assert.AreEqual(model.GetValueOrDefault(x => x.LogErrors).AsHtmlString(), propertiesTableDictionary["Log Errors:"]);
			Assert.AreEqual(model.GetValueOrDefault(x => x.EmailNotifications), propertiesTableDictionary["Email Notification Recipients:"]);
			Assert.AreEqual(model.GetValueOrDefault(x => x.IncludeInEcaPromote).AsHtmlString(), propertiesTableDictionary["Included in ECA Promote List:"]);
			// todo total of docs
			// todo total of imgs
			Assert.AreEqual(model.GetValueOrDefault(x => x.CreateSavedSearch).AsHtmlString(), propertiesTableDictionary["Create Saved Search:"]);
		}

		protected static string GetExpectedExportType(RelativityProviderModel model)
		{
			string expectedStr = "Workspace;";
			if (model.ImagePrecedence.HasValue)
			{
				expectedStr += "Images;";
			}

			if (model.CopyFilesToRepository == true || model.CopyNativeFiles.HasValue)
			{
				expectedStr += "Natives;";
			}

			return expectedStr;
		}

		protected static string GetExpectedSourceDetails(RelativityProviderModel model)
		{
			string sourceType = SourceTypeEnumToString(model.GetValueOrDefault(x => x.Source));
			string savedSearchName = model.GetValueOrDefault(x => x.SavedSearch);  //TODO: only supports saved searches

			return $"{sourceType}: {savedSearchName}";
		}

		protected static string SourceTypeEnumToString(RelativityProviderModel.SourceTypeEnum? value)
		{
			switch (value)
			{
				case RelativityProviderModel.SourceTypeEnum.Production:
					return "Production";
				case RelativityProviderModel.SourceTypeEnum.SavedSearch:
					return "Saved Search";
				default: return "";
			}
		}

		protected static string OverwriteModeEnumToString(RelativityProviderModel.OverwriteModeEnum? value)
		{
			switch (value)
			{
				case RelativityProviderModel.OverwriteModeEnum.AppendOnly:
					return "Append Only";
				case RelativityProviderModel.OverwriteModeEnum.OverlayOnly:
					return "Overlay Only";
				case RelativityProviderModel.OverwriteModeEnum.AppendOverlay:
					return "Append/Overlay";
				default:
					return "";
			}
		}

		protected static string UseFolderPathInformationEnumToString(RelativityProviderModel.UseFolderPathInformationEnum? value)
		{
			switch (value)
			{
				case RelativityProviderModel.UseFolderPathInformationEnum.No:
					return "No";
				case RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField:
					return "Read From Field:Document Folder Path"; // Document Folder Path is hardcoded in IntegrationPointAction
				case RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree:
					return "Read From Folder Tree";
				default:
					return "";
			}
		}

		protected static string ImagePrecedenceEnumToString(ImagePrecedenceEnum? value)
		{
			switch (value)
			{
				case ImagePrecedenceEnum.OriginalImages:
					return "Original";
				case ImagePrecedenceEnum.ProducedImages:
					return "Produced";
				default:
					return "";
			}
		}

		protected static string MultiSelectFieldOverlayBehaviorEnumToString(RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum? value)
		{
			switch (value)
			{
				case RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.MergeValues:
					return "Merge Values";
				case RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.ReplaceValues:
					return "Replace Values";
				case RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings:
					return "Use Field Settings";
				default: return "";
			}
		}
	}
}