using kCura.IntegrationPoints.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeInstanceSettingRepository: IInstanceSettingRepository
    {
        private const string _LONG_TEXT_LIMIT_SECTION = "kCura.EDDS.Web";
        private const string _LONG_TEXT_LIMIT_NAME = "MaximumNumberOfCharactersSupportedByLongText";
        private const string _LONG_TEXT_LIMIT_VALUE = "100000";

        public string GetConfigurationValue(string section, string name)
        {
            if (section == _LONG_TEXT_LIMIT_SECTION && name == _LONG_TEXT_LIMIT_NAME)
            {
                return _LONG_TEXT_LIMIT_VALUE;
            }
            else
            {
                return null;
            }      
        }
    }
}
