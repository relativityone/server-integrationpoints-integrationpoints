using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts;

//http://platform.kcura.com/9.0/index.htm#Customizing_workflows/Page_Interaction_event_handlers.htm?Highlight=javascript

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public abstract class PageInteractionEventHandler : EventHandler.PageInteractionEventHandler
    {
        public abstract ICommonScriptsFactory CommonScriptsFactory { get; set; }

        public override FieldCollection RequiredFields
        {
            get
            {
                var fieldCollection = new FieldCollection
                {
                    new Field(IntegrationPointFields.DestinationProvider),
                    new Field(IntegrationPointFields.SourceProvider),
                    new Field(IntegrationPointFields.DestinationConfiguration),
                    new Field(IntegrationPointFields.SourceConfiguration)
                };
                return fieldCollection;
            }
        }

        public override Response PopulateScriptBlocks()
        {
            Response response = new Response
            {
                Success = true,
                Message = string.Empty
            };


            if (PageMode == EventHandler.Helper.PageMode.View)
            {
                string applicationPath = PageInteractionHelper.GetApplicationRelativeUri();

                ICommonScripts commonScripts = CommonScriptsFactory.Create(this);

                foreach (var css in commonScripts.LinkedCss())
                {
                    RegisterLinkedCss(applicationPath + css);
                }

                foreach (var script in commonScripts.LinkedScripts())
                {
                    RegisterLinkedClientScript(applicationPath + script);
                }

                foreach (var scriptBlock in commonScripts.ScriptBlocks())
                {
                    RegisterClientScriptBlock(new ScriptBlock
                    {
                        Key = Guid.NewGuid().ToString(),
                        Script = scriptBlock
                    });
                }
            }
            return response;
        }
    }
}