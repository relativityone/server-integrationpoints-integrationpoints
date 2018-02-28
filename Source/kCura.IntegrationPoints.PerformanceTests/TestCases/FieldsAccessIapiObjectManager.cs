using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.PerformanceTests.TestCases
{
	[ClrJob]
	[RPlotExporter, RankColumn]
	public class FieldsAccessIapiObjectManager
	{
		private TextWriter _previousConsoleOutput;
		private SutOld _sutOld;
		private SutNew _sutNew;

		[GlobalSetup]
		public void Setup()
		{
			SetupConsoleOutput();
			SetupAppConfig();

			_sutOld = new SutOld();
			_sutOld.Init();

			_sutNew = new SutNew();
			_sutNew.Init();
		}

		private static void SetupAppConfig()
		{
			string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string originalAppConfigName = "kCura.IntegrationPoints.PerformanceTests.exe.config";
			string appConfigPath = Path.Combine(executingDirectory, originalAppConfigName);

			AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", appConfigPath);
		}

		private void SetupConsoleOutput()
		{
			// we don't want to show output from test on console
			_previousConsoleOutput = Console.Out;
			Console.SetOut(new StreamWriter(new MemoryStream()));
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			Console.SetOut(_previousConsoleOutput);
		}

		[Params(false, true)]
		public bool LongTextFieldsOnly { get; set; }

		[Benchmark]
		public void Old()
		{
			_sutOld.Execute(LongTextFieldsOnly);
		}

		[Benchmark]
		public void New()
		{
			_sutNew.Execute(LongTextFieldsOnly);
		}
	}

	internal class SutOld : SourceProviderTemplate
	{
		private const string WORKSPACE_NAME = "Field access performance - old";

		private IRSAPIClient _client;
		private IChoiceService _choiceService;

		public SutOld() : base(WORKSPACE_NAME)
		{ }

		public void Init()
		{
			SuiteSetup();

			var rsapiClientWithWorkspaceFactory = Container.Resolve<IRsapiClientWithWorkspaceFactory>();
			_client = rsapiClientWithWorkspaceFactory.CreateUserClient(WorkspaceArtifactId);
			_choiceService = Container.Resolve<IChoiceService>();
		}

		public void Execute(bool longTextFieldsOnly)
		{
			OldGetTextFields(Convert.ToInt32(ArtifactType.Document), longTextFieldsOnly);
		}

		private List<FieldEntry> OldGetTextFields(int rdoTypeId, bool longTextFieldsOnly)
		{
			var rdoCondition = new ObjectCondition
			{
				Field = Constants.Fields.ObjectTypeArtifactTypeId,
				Operator = ObjectConditionEnum.AnyOfThese,
				Value = new List<int> { rdoTypeId }
			};

			var longTextCondition = new TextCondition
			{
				Field = Constants.Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = FieldTypes.LongText
			};

			var fixedLengthTextCondition = new TextCondition
			{
				Field = Constants.Fields.FieldType,
				Operator = TextConditionEnum.EqualTo,
				Value = FieldTypes.FixedLengthText
			};

			var query = new Query
			{
				ArtifactTypeName = "Field",
				Fields = new List<Field>(),
				Sorts = new List<Sort>
				{
					new Sort
					{
						Field = Constants.Fields.Name,
						Direction = SortEnum.Ascending
					}
				}
			};
			var documentLongTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, longTextCondition);
			var documentFixedLengthTextCondition = new CompositeCondition(rdoCondition, CompositeConditionEnum.And, fixedLengthTextCondition);
			query.Condition = longTextFieldsOnly ? documentLongTextCondition : new CompositeCondition(documentLongTextCondition, CompositeConditionEnum.Or, documentFixedLengthTextCondition);

			QueryResult result = _client.Query(_client.APIOptions, query);

			if (!result.Success)
			{
				throw new Exception(result.Message);
			}
			List<FieldEntry> fieldEntries = _choiceService.ConvertToFieldEntries(result.QueryArtifacts);
			return fieldEntries;
		}
	}

	internal class SutNew : SourceProviderTemplate
	{
		private const string WORKSPACE_NAME = "Field access performance - new";
		private FieldService _fieldService;

		public SutNew() : base(WORKSPACE_NAME)
		{ }

		public void Init()
		{
			SuiteSetup();
			var choiceService = Container.Resolve<IChoiceService>();
			var client = Container.Resolve<IRSAPIClient>();
			_fieldService = new FieldService(choiceService, client);
		}

		public void Execute(bool longTextFieldsOnly)
		{
			_fieldService.GetTextFields(Convert.ToInt32(ArtifactType.Document), longTextFieldsOnly);
		}
	}
}
