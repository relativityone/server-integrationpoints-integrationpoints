using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
    public class RelativityProviderScripts : CommonScripts
    {
        private readonly IFolderPathInformation _folderPathInformation;

        public RelativityProviderScripts(ScriptsHelper scriptsHelper, IIntegrationPointBaseFieldGuidsConstants guidsConstants, IFolderPathInformation folderPathInformation)
            : base(scriptsHelper, guidsConstants)
        {
            _folderPathInformation = folderPathInformation;
        }

        public override IList<string> LinkedScripts()
        {
            var result = base.LinkedScripts();
            result.Add("/Scripts/moment.js");
            result.Add("/Scripts/EventHandlers/relativity-provider-view.js");
            return result;
        }

        public override IList<string> ScriptBlocks()
        {
            var result = base.ScriptBlocks();

            string folderPathInformation = _folderPathInformation.RetrieveName(ScriptsHelper.GetDestinationConfiguration());

            string block = "<script>" +
                            $"IP.fieldName = '{folderPathInformation}';" +
                            "</script>";

            result.Add(block);

            return result;
        }
    }
}
