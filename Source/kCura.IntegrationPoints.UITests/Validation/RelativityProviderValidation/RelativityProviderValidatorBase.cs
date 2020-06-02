using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoints.UITests.Tests;
using NUnit.Framework;
using Relativity.Kepler.Logging;
using Serilog;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation
{
	public abstract class RelativityProviderValidatorBase : BaseUiValidator
	{
		public void ValidateSummaryPage(PropertiesTable propertiesTable, RelativityProviderModel model, TestContext sourceContext, TestContext destinationContext, bool expectErrors)
		{
			var callDurationStopWatch = new Stopwatch();
			callDurationStopWatch.Start();
			Dictionary<string, string> propertiesTableDictionary = propertiesTable.Properties;

			ValidateHasErrorsProperty(propertiesTableDictionary, expectErrors);
			ValidateGeneralModel(propertiesTableDictionary, model, sourceContext, destinationContext);
			callDurationStopWatch.Stop();
			Log.Information("Validation finished. Duration: {duration} s", callDurationStopWatch.ElapsedMilliseconds/1000);
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
			// todo total of docs
			// todo total of imgs
			Assert.AreEqual(model.GetValueOrDefault(x => x.CreateSavedSearch).AsHtmlString(), propertiesTableDictionary["Create Saved Search:"]);
		}

		protected static string GetExpectedExportType(RelativityProviderModel model)
		{
			string expectedStr = "Workspace;";
			if (model.ImagePrecedence.HasValue || model.CopyImages)
			{
				expectedStr += "Images;";
			}

			if (model.CopyFilesToRepository == true || model.CopyNativeFiles != null && model.CopyNativeFiles != RelativityProviderModel.CopyNativeFilesEnum.No)
			{
				expectedStr += "Natives;";
			}

			return expectedStr;
		}

		protected static string GetExpectedSourceDetails(RelativityProviderModel model)
		{
			string sourceType = SourceTypeEnumToString(model.GetValueOrDefault(x => x.Source));
			string sourceDetails = SourceDetailsToString(model);

			return $"{sourceType}: {sourceDetails}";
		}

		protected static string SourceTypeEnumToString(RelativityProviderModel.SourceTypeEnum? value)
		{
			switch (value)
			{
				case RelativityProviderModel.SourceTypeEnum.Production:
					return "Production Set";
				case RelativityProviderModel.SourceTypeEnum.SavedSearch:
					return "Saved Search";
				default: return "";
			}
		}

		protected static string SourceDetailsToString(RelativityProviderModel model)
		{
			switch (model.Source)
			{
				case RelativityProviderModel.SourceTypeEnum.Production:
					return model.GetValueOrDefault(x => x.SourceProductionName);
				case RelativityProviderModel.SourceTypeEnum.SavedSearch:
					return model.GetValueOrDefault(x => x.SavedSearch);
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

		protected static string ImagePrecedenceToString(RelativityProviderModel model)
		{
			ImagePrecedence value = model.GetValueOrDefault(x => x.ImagePrecedence);
			
			switch (value)
			{
				case ImagePrecedence.OriginalImages:
					return "Original";
				case ImagePrecedence.ProducedImages:
					return $"Produced: {model.SourceProductionName}";
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