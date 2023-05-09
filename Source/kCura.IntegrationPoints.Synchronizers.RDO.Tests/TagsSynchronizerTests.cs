using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Readers;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture, Category("Unit")]
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

            _instance = new TagsSynchronizer(_helper, _dataSynchronizer, new JSONSerializer());
        }

        [Test]
        [Combinatorial]
        public void ItShouldUpdateImportSettingsForSyncData([Values(true, false)] bool imageImport, [Values(true, false)] bool productionImport,
            [Values(true, false)] bool useDynamicFolderPath)
        {
            var destinationConfiguration = new DestinationConfiguration()
            {
                ImageImport = imageImport,
                ProductionImport = productionImport,
                UseDynamicFolderPath = useDynamicFolderPath
            };

            // ACT
            _instance.SyncData(_data, _fieldMap, new ImportSettings(destinationConfiguration), null, new EmptyDiagnosticLog());

            // ASSERT
            _dataSynchronizer.Received(1).SyncData(_data, _fieldMap, Arg.Is<ImportSettings>(x => AssertOptions(x.DestinationConfiguration)), null, Arg.Any<IDiagnosticLog>());
        }

        [Test]
        [Combinatorial]
        public void ItShouldUpdateImportSettingsForSyncData_Records([Values(true, false)] bool imageImport, [Values(true, false)] bool productionImport,
            [Values(true, false)] bool useDynamicFolderPath)
        {
            var destinationConfiguration = new DestinationConfiguration()
            {
                ImageImport = imageImport,
                ProductionImport = productionImport,
                UseDynamicFolderPath = useDynamicFolderPath
            };

            // ACT
            _instance.SyncData(_records, _fieldMap, new ImportSettings(destinationConfiguration), (IJobStopManager)null, null);

            // ASSERT
            _dataSynchronizer.Received(1).SyncData(_records, _fieldMap, Arg.Is<ImportSettings>(x => AssertOptions(x.DestinationConfiguration)), null, null);
        }

        [Test]
        [Combinatorial]
        public void ItShouldUpdateImportSettingsForGetFields([Values(true, false)] bool imageImport, [Values(true, false)] bool productionImport,
            [Values(true, false)] bool useDynamicFolderPath)
        {
            var destinationConfiguration = new DestinationConfiguration()
            {
                ImageImport = imageImport,
                ProductionImport = productionImport,
                UseDynamicFolderPath = useDynamicFolderPath
            };

            // ACT
            _instance.GetFields(new DataSourceProviderConfiguration(JsonConvert.SerializeObject(new ImportSettings(destinationConfiguration))));

            // ASSERT
            _dataSynchronizer.Received(1).GetFields(Arg.Is<DataSourceProviderConfiguration>(x => AssertOptions(x.Configuration)));
        }

        private bool AssertOptions(string settings)
        {
            return AssertOptions(JsonConvert.DeserializeObject<DestinationConfiguration>(settings));
        }

        private bool AssertOptions(DestinationConfiguration importSettings)
        {
            return !importSettings.ImageImport &&
                   !importSettings.ProductionImport &&
                   !importSettings.UseDynamicFolderPath;
        }
    }
}
