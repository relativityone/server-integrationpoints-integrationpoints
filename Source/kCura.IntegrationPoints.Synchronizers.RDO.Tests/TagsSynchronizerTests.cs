﻿using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	public class TagsSynchronizerTests : TestBase
	{
		private IEnumerable<FieldMap> _fieldMap;
		private IEnumerable<IDictionary<FieldEntry, object>> _records;
		private IDataTransferContext _data;
		private IDataSynchronizer _dataSynchronizer;
		private IHelper _helper;

		private TagsSynchronizer _instance;

		public override void SetUp()
		{
			_fieldMap = Substitute.For<IEnumerable<FieldMap>>();
			_records = Substitute.For<IEnumerable<IDictionary<FieldEntry, object>>>();
			_data = Substitute.For<IDataTransferContext>();
			_dataSynchronizer = Substitute.For<IDataSynchronizer>();
			_helper = Substitute.For<IHelper>();

			_instance = new TagsSynchronizer(_helper,_dataSynchronizer);
		}

		[Test]
		[Combinatorial]
		public void ItShouldUpdateImportSettingsForSyncData([Values(true, false)] bool imageImport, [Values(true, false)] bool productionImport,
			[Values(true, false)] bool useDynamicFolderPath)
		{
			ImportSettings importSettings = new ImportSettings
			{
				ImageImport = imageImport,
				ProductionImport = productionImport,
				UseDynamicFolderPath = useDynamicFolderPath
			};

			// ACT
			_instance.SyncData(_data, _fieldMap, JsonConvert.SerializeObject(importSettings));

			// ASSERT
			_dataSynchronizer.Received(1).SyncData(_data, _fieldMap, Arg.Is<string>(x => AssertOptions(x)));
		}

		[Test]
		[Combinatorial]
		public void ItShouldUpdateImportSettingsForSyncData_Records([Values(true, false)] bool imageImport, [Values(true, false)] bool productionImport,
			[Values(true, false)] bool useDynamicFolderPath)
		{
			ImportSettings importSettings = new ImportSettings
			{
				ImageImport = imageImport,
				ProductionImport = productionImport,
				UseDynamicFolderPath = useDynamicFolderPath
			};

			// ACT
			_instance.SyncData(_records, _fieldMap, JsonConvert.SerializeObject(importSettings));

			// ASSERT
			_dataSynchronizer.Received(1).SyncData(_records, _fieldMap, Arg.Is<string>(x => AssertOptions(x)));
		}

		[Test]
		[Combinatorial]
		public void ItShouldUpdateImportSettingsForGetFields([Values(true, false)] bool imageImport, [Values(true, false)] bool productionImport,
			[Values(true, false)] bool useDynamicFolderPath)
		{
			ImportSettings importSettings = new ImportSettings
			{
				ImageImport = imageImport,
				ProductionImport = productionImport,
				UseDynamicFolderPath = useDynamicFolderPath
			};

			// ACT
			_instance.GetFields(new DataSourceProviderConfiguration(JsonConvert.SerializeObject(importSettings)));

			// ASSERT
			_dataSynchronizer.Received(1).GetFields(Arg.Is<DataSourceProviderConfiguration>(x => AssertOptions(x.Configuration)));
		}

		private bool AssertOptions(string s)
		{
			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(s);
			return !importSettings.ImageImport && !importSettings.ProductionImport && !importSettings.UseDynamicFolderPath;
		}
	}
}