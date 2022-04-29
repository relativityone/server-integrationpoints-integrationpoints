using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal class SourceWorkspaceDataReaderCommonTests : SourceWorkspaceDataReaderTestsBase
	{
		private const char _RECORD_SEPARATOR = (char)30;
		private const int _SINGLE_OBJECT_ARTIFACT_ID = 1;
		private const int _USER_ARTIFACT_ID = 9;
		private const string _USER_FULL_NAME = "Admin, Relativity";
		private const string _USER_EMAIL = "relativity.admin@kcura.com";

		private static IEnumerable<TestCaseData> ReturnCorrectValueForDifferentDataTypesTestCases()
		{
			yield return new TestCaseData(RelativityDataType.Currency, 1.0f)
				.Returns("1");

			DateTime expectedDateTime = new DateTime(2019, 6, 26);
			yield return new TestCaseData(RelativityDataType.Date, expectedDateTime)
				.Returns(expectedDateTime.ToString(CultureInfo.CurrentCulture));

			yield return new TestCaseData(RelativityDataType.Decimal, 2.0)
				.Returns("2");

			yield return new TestCaseData(RelativityDataType.File, "C:\\xd\\Import_03SmallNatives.zip")
				.Returns("C:\\xd\\Import_03SmallNatives.zip");

			yield return new TestCaseData(RelativityDataType.FixedLengthText, "Test1234")
				.Returns("Test1234");

			yield return new TestCaseData(RelativityDataType.LongText, "Test12345")
				.Returns("Test12345");

			yield return new TestCaseData(RelativityDataType.MultipleObject, GenerateMultipleObject("Test1", "Foo", "Bar Baz"))
				.Returns($"Test1{_RECORD_SEPARATOR}Foo{_RECORD_SEPARATOR}Bar Baz");

			yield return new TestCaseData(RelativityDataType.SingleChoice, GenerateSingleChoice("Cool Choice"))
				.Returns("Cool Choice");

			yield return new TestCaseData(RelativityDataType.SingleObject, GenerateSingleObject("Cool Object"))
				.Returns("Cool Object");

			yield return new TestCaseData(RelativityDataType.User, GenerateUser(_USER_FULL_NAME))
				.Returns(_USER_EMAIL);

			yield return new TestCaseData(RelativityDataType.WholeNumber, 15)
				.Returns("15");

			yield return new TestCaseData(RelativityDataType.YesNo, true)
				.Returns("True");
		}

		[TestCaseSource(nameof(ReturnCorrectValueForDifferentDataTypesTestCases))]
		public async Task<object> Read_ShouldReturnCorrectValue_ForDifferentBasicDataTypesAndMapFieldsCorrectly(RelativityDataType dataType, object initialValue)
		{
			// Arrange
			const int blockSize = 1;
			SetUp(blockSize);

			string sourceColumnName = dataType.ToString();
			string destinationColumnName = $"{sourceColumnName}_123";
			const string destinationIdentifierColumnName = "Alt Letter";

			HashSet<FieldConfiguration> fields = IdentifierWithSpecialFields(_DEFAULT_IDENTIFIER_COLUMN_NAME, destinationIdentifierColumnName);
			fields.Add(FieldConfiguration.Regular(sourceColumnName, destinationColumnName, dataType, initialValue));

			DocumentImportJob importData = CreateDefaultDocumentImportJob(blockSize, CreateDocumentForNativesTransfer, fields);
			_configuration.SetFieldMappings(importData.FieldMappings);
			await _documentTransferServicesMocker.SetupServicesWithNativesTestDataAsync(importData, blockSize).ConfigureAwait(false);

			// Act
			_instance.Read();

			// Assert

			// Check identifier field
			Document document = importData.Documents.First();
			object expectedIdentifier = document.FieldValues.First(x => x.Field == _DEFAULT_IDENTIFIER_COLUMN_NAME).Value;
			_instance[destinationIdentifierColumnName].Should().Be(expectedIdentifier);

			// Check special fields
			foreach (FieldConfiguration specialField in DefaultSpecialFields)
			{
				object expectedValue = specialField.Value.ToString();
				_instance[specialField.DestinationColumnName].Should().Be(expectedValue);
			}

			// Check regular field
			return _instance[destinationColumnName];
		}

		[TestCaseSource(nameof(ApiCallsFailureSetups))]
		public async Task Read_ShouldThrowSourceDataReaderException_WhenApiCallFails(Action<DocumentTransferServicesMocker> failureSetup)
		{
			// Arrange
			const int batchSize = 500;
			const int blockSize = 300;
			SetUp(batchSize);

			DocumentImportJob importData = CreateDefaultDocumentImportJob(batchSize, CreateDocumentForNativesTransfer, DefaultIdentifierWithSpecialFields);
			await _documentTransferServicesMocker.SetupServicesWithNativesTestDataAsync(importData, blockSize).ConfigureAwait(false);

			failureSetup(_documentTransferServicesMocker);

			// Act/Assert
			Assert.Throws<SourceDataReaderException>(() => importData.Documents.ForEach(x => _instance.Read()));
		}

		private static IEnumerable<TestCaseData> ApiCallsFailureSetups()
		{
			Tuple<Action<DocumentTransferServicesMocker>, string>[] failureActionAndNamePairs =
			{
				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCreation(), "Failing object manager creation"),
				
				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCall(om =>
						om.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(), It.Is<int>(x => x == 0))),
					"Failing first RetrieveResultsBlockFromExportAsync object manager call"),

				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCall(om =>
						om.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.Is<int>(x => x == 200), It.Is<int>(x => x == 300))),
					"Failing second RetrieveResultsBlockFromExportAsync object manager call"),

				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingSearchServiceCall(fm =>
						fm.RetrieveNativesForSearchAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())),
					"Failing GetNativesForSearchAsync search manager call"),
                
				new Tuple<Action<DocumentTransferServicesMocker>, string>(dtsm => dtsm.SetupFailingObjectManagerCall(om =>
						om.QuerySlimAsync(It.IsAny<int>(), It.Is<QueryRequest>(r => r.ObjectType.Name == "Field"), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())),
					"Failing QuerySlimAsync object manager call")

			};

			return failureActionAndNamePairs.Select(fa => new TestCaseData(fa.Item1) { TestName = fa.Item2 });
		}

		private static object GenerateSingleChoice(string name)
		{
			return RunThroughSerializer(new { Name = name });
		}

		private static object GenerateUser(string fullName)
		{
			return RunThroughSerializer(new { ArtifactID = _USER_ARTIFACT_ID, Name = fullName });
		}

		private static object GenerateSingleObject(string name)
		{
			return RunThroughSerializer(new { ArtifactID = _SINGLE_OBJECT_ARTIFACT_ID, Name = name });
		}

		private static object GenerateMultipleObject(params string[] names)
		{
			return RunThroughSerializer(names.Select(x => new { Name = x }).ToArray());
		}

		protected override IBatchDataReaderBuilder CreateBatchDataReaderBuilder()
		{
			return new NativeBatchDataReaderBuilder(_container.Resolve<IFieldManager>(), _container.Resolve<IExportDataSanitizer>(), new EmptyLogger());
		}
	}
}