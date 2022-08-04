﻿using System;
using kCura.EventHandler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public abstract class PreLoadEventHandler : EventHandler.PreLoadEventHandler
    {
        public abstract IIntegrationPointViewPreLoad IntegrationPointViewPreLoad { get; set; }

        public override FieldCollection RequiredFields
        {
            get
            {
                var fieldCollection = new FieldCollection
                {
                    new Field(IntegrationPointFields.SourceConfiguration),
                    new Field(IntegrationPointFields.SourceProvider)
                };
                return fieldCollection;
            }
        }

        public override Response Execute()
        {
            var response = new Response
            {
                Success = true,
                Message = ""
            };

            try
            {
                if (PageMode == EventHandler.Helper.PageMode.View)
                {
                    IntegrationPointViewPreLoad.PreLoad(ActiveArtifact);
                }
            }
            catch (Exception e)
            {
                var errorMessage = "Failed to execute PreLoadEventHandler";
                Helper.GetLoggerFactory().GetLogger().ForContext<PreLoadEventHandler>().LogError(e, errorMessage);
                response.Exception = e;
                response.Success = false;
                response.Message = errorMessage;
            }

            return response;
        }
    }
}