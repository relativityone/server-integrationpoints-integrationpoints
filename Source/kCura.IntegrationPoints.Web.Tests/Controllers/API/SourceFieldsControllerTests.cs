using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Controllers;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture]
    public class SourceFieldsControllerTests : TestBase
    {
        private SourceFieldsController _instance;
        private HttpConfiguration _configuration;
        private SourceProvider _providerRdo;

        private IHelper _helper;
        private IDataSourceProvider _dataSourceProvider;
        private IDataProviderFactory _factory;
        private IManagerFactory _managerFactory;
        private IContextContainerFactory _contextContainerFactory;
        private ICaseServiceContext _caseService;
        private IGetSourceProviderRdoByIdentifier _sourceProviderIdentifier;

        private FieldEntry _fieldA, _fieldB, _fieldC, _fieldD = null;
        private readonly Guid _dataType = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
        private readonly Guid _appIdentifier = new Guid("00000000-0000-0000-0000-000000000000");
        private const string _options = "TestOptions";
	    private const string _credentials = "Credentials";

        [SetUp]
        public override void SetUp()
        {
            _helper = Substitute.For<IHelper>();
            _factory = Substitute.For<IDataProviderFactory>();
            _managerFactory = Substitute.For<IManagerFactory>();
            _contextContainerFactory = Substitute.For<IContextContainerFactory>();
            _caseService = Substitute.For<ICaseServiceContext>();
            _sourceProviderIdentifier = Substitute.For<IGetSourceProviderRdoByIdentifier>();
            _providerRdo = new Data.SourceProvider() { ApplicationIdentifier = _appIdentifier.ToString() };
            _dataSourceProvider = Substitute.For<IDataSourceProvider>();
            _configuration = Substitute.For<HttpConfiguration>();

            _fieldA = new FieldEntry() { FieldIdentifier = "AAA", DisplayName = "aaa" };
            _fieldB = new FieldEntry() { FieldIdentifier = "BBB", DisplayName = "bbb" };
            _fieldC = new FieldEntry() { FieldIdentifier = "CCC", DisplayName = "ccc" };
            _fieldD = new FieldEntry() { FieldIdentifier = "DDD", DisplayName = "ddd" };

            _dataSourceProvider.GetFields(Arg.Is<DataSourceProviderConfiguration>(x => x.Configuration.Equals(_options) && x.SecuredConfiguration.Equals(_credentials))).Returns(new List<FieldEntry>
            {
                _fieldD, _fieldB, _fieldA, _fieldC
            });
            _factory.GetDataProvider(_appIdentifier, _dataType)
                .Returns(_dataSourceProvider);
            _sourceProviderIdentifier.Execute(Guid.Empty).ReturnsForAnyArgs(_providerRdo);

            _instance = new SourceFieldsController(_sourceProviderIdentifier, _caseService, _contextContainerFactory, _managerFactory, _factory, _helper)
            {
                Configuration =  _configuration,
                Request = new System.Net.Http.HttpRequestMessage()
            };
            _instance.Configuration = _configuration;

        }

        [TestCase]
        public void GetSourceFieldsGoldFlow()
        {
            var response = _instance.Get(new SourceOptions() { Options  = _options, Type = _dataType, Credentials = _credentials });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(new List<FieldEntry>() { _fieldA, _fieldB, _fieldC, _fieldD }, (List<FieldEntry>)((System.Net.Http.ObjectContent<List<FieldEntry>>)response.Content).Value);

            _factory.Received().GetDataProvider(_appIdentifier, _dataType);
            _dataSourceProvider.Received(1).GetFields(Arg.Is<DataSourceProviderConfiguration>(x => x.Configuration.Equals(_options) && x.SecuredConfiguration.Equals(_credentials)));
            _sourceProviderIdentifier.Received(1).Execute(_dataType);
        }
    }
}
