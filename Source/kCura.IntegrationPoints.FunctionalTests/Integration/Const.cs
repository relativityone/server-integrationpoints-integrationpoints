﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public static class Const
    {
        public const string INTEGRATION_POINTS_APP_GUID = "DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";

        public static int OVERWRITE_FIELD_ARTIFACT_ID = ArtifactProvider.NextId();
        public static int FIXED_LENGTH_TEXT_TYPE_ARTIFACT_ID = ArtifactProvider.NextId();
        public static int LONG_TEXT_TYPE_ARTIFACT_ID = ArtifactProvider.NextId();
        public static int INTEGRATION_POINTS_ARTIFACT_ID = ArtifactProvider.NextId();

        public static class Agent
        {
            public static readonly Guid RELATIVITY_INTEGRATION_POINTS_AGENT_GUID =
                new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

            public static readonly int INTEGRATION_POINTS_AGENT_TYPE_ID = ArtifactProvider.NextId();

            public static readonly List<int> RESOURCE_GROUP_IDS = new List<int>
                {ArtifactProvider.NextId(), ArtifactProvider.NextId(), ArtifactProvider.NextId()};
        }

        public static class Provider
        {
            public const string _MY_FIRST_PROVIDER = "C9DE331D-2DCA-4F78-85BD-91493D0B9B37";
        }

        public static class Document
        {
            public static readonly string _DOCUMENT_NAME = "Document";
            public static readonly Guid _DOCUMENT_GUID = new Guid("2A3F1212-C8CA-4FA9-AD6B-F76C97F05438");

        }


        public static class Entity
        {
            public static readonly string _ENTITY_OBJECT_NAME = "Entity";
            public static readonly string _ENTITY_OBJECT_FIRST_NAME = "First Name";
            public static readonly string _ENTITY_OBJECT_LAST_NAME = "Last Name";
            public static readonly int _ENTITY_TYPE_ARTIFACT_ID = ArtifactProvider.NextId();
            public static readonly Guid _ENTITY_OBJECT_GUID = new Guid("D216472D-A1AA-4965-8B36-367D43D4E64C");
        }

        public static class Choices
        {
            public static readonly List<global::Relativity.Services.ChoiceQuery.Choice> OverwriteFields =
                new List<global::Relativity.Services.ChoiceQuery.Choice>()
                {
                        new global::Relativity.Services.ChoiceQuery.Choice()
                        {
                            ArtifactID = 1039894,
                            Name = "Append Only"
                        },
                        new global::Relativity.Services.ChoiceQuery.Choice()
                        {
                            ArtifactID = 1039895,
                            Name = "Append/Overlay"
                        },
                        new global::Relativity.Services.ChoiceQuery.Choice()
                        {
                            ArtifactID = 1039896,
                            Name = "Overlay Only"
                        },
                        new global::Relativity.Services.ChoiceQuery.Choice()
                        {
                            ArtifactID = 1039897,
                            Name = "Yes"
                        },
                        new global::Relativity.Services.ChoiceQuery.Choice()
                        {
                            ArtifactID = 1039898,
                            Name = "No"
                        }
                };
        }

        public static class RdoGuids
        {
            public static class SyncConfiguration
            {
                public const string JobHistoryId = "Job History ID";
                public static readonly Guid JobHistoryIdGuid = new Guid("FF793525-29AB-40B2-A8AE-88E574EAD0DE");
                public const string Resuming = "Resuming";
                public static readonly Guid ResumingGuid = new Guid("A5B0959D-96CE-4CA0-9E4A-576132F29165");
            }

            public static class JobHistory
            {
                public static readonly List<Guid> Guids = new List<Guid>
                {
                    JobHistoryFieldGuids.DocumentsGuid,
                    JobHistoryFieldGuids.IntegrationPointGuid,
                    JobHistoryFieldGuids.JobStatusGuid,
                    JobHistoryFieldGuids.ItemsTransferredGuid,
                    JobHistoryFieldGuids.ItemsWithErrorsGuid,
                    JobHistoryFieldGuids.StartTimeUTCGuid,
                    JobHistoryFieldGuids.EndTimeUTCGuid,
                    JobHistoryFieldGuids.BatchInstanceGuid,
                    JobHistoryFieldGuids.DestinationWorkspaceGuid,
                    JobHistoryFieldGuids.TotalItemsGuid,
                    JobHistoryFieldGuids.DestinationWorkspaceInformationGuid,
                    JobHistoryFieldGuids.JobTypeGuid,
                    JobHistoryFieldGuids.DestinationInstanceGuid,
                    JobHistoryFieldGuids.FilesSizeGuid,
                    JobHistoryFieldGuids.OverwriteGuid,
                    JobHistoryFieldGuids.JobIDGuid,
                    JobHistoryFieldGuids.NameGuid,
                };
            }

            public static class JobHistoryError
            {
                public static readonly List<Guid> Guids = new List<Guid>
                {
                    JobHistoryErrorFieldGuids.JobHistoryGuid,
                    JobHistoryErrorFieldGuids.ErrorTypeGuid,
                    JobHistoryErrorFieldGuids.NameGuid
                };
            }

            public static class Entity
            {
                public static readonly List<Guid> Guids = new List<Guid>
                {
                    EntityFieldGuids.UniqueIdGuid,
                    EntityFieldGuids.FirstNameGuid,
                    EntityFieldGuids.LastNameGuid,
                    EntityFieldGuids.FullNameGuid,
                    EntityFieldGuids.ManagerGuid
                };
            }
        }
        public static class FieldOverlayBehaviorName
        {
            public const string USE_FIELD_SETTINGS = "Use Field Settings";
            public const string MERGE_VALUES = "Merge Values";
            public const string REPLACE_VALUES = "Replace Values";
        }
    }
}
