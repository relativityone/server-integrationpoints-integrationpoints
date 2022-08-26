﻿using System;
using kCura.EventHandler;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    public interface IIntegrationPointViewPreLoad
    {
        void PreLoad(Artifact artifact);

        void ResetSavedSearchArtifactId(
            Action<Artifact> initializeAction,
            Artifact artifact,
            IEHHelper helper);
    }
}