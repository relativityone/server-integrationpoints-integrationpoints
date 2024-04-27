using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Executors.Tagging
{
    internal class TaggingRepository : ITaggingRepository
    {
        private readonly Guid _destinationWorkspaceTagGuid = new Guid("8980C2FA-0D33-4686-9A97-EA9D6F0B4196");
        private readonly Guid _jobHistoryGuid = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");

        private readonly IProxyFactoryDocument _serviceFactory;

        public TaggingRepository(IProxyFactoryDocument serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<MassUpdateResult> TagDocumentsAsync(int workspaceId, List<int> documentsIds, int destinationWorkspaceTagId, int jobHistoryId)
        {
            IObjectManager objectManager = await _serviceFactory.CreateProxyDocumentAsync<IObjectManager>(Identity.System).ConfigureAwait(false);

            var massUpdateOptions = new MassUpdateOptions
            {
                UpdateBehavior = FieldUpdateBehavior.Merge
            };

            var updateByIdentifiersRequest = new MassUpdateByObjectIdentifiersRequest
            {
                Objects = ConvertArtifactIdsToObjectRefs(documentsIds),
                FieldValues = GetDocumentFieldTags(destinationWorkspaceTagId, jobHistoryId)
            };

            return await objectManager.UpdateAsync(workspaceId, updateByIdentifiersRequest, massUpdateOptions).ConfigureAwait(false);
        }

        private static IReadOnlyList<RelativityObjectRef> ConvertArtifactIdsToObjectRefs(IList<int> artifactIds)
        {
            var objectRefs = new RelativityObjectRef[artifactIds.Count];

            for (int i = 0; i < artifactIds.Count; i++)
            {
                var objectRef = new RelativityObjectRef
                {
                    ArtifactID = artifactIds[i]
                };
                objectRefs[i] = objectRef;
            }

            return objectRefs;
        }

        private FieldRefValuePair[] GetDocumentFieldTags(int destinationWorkspaceTagId, int jobHistoryId)
        {
            FieldRefValuePair[] fieldRefValuePairs =
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _destinationWorkspaceTagGuid },
                    Value = ToMultiObjectValue(destinationWorkspaceTagId)
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _jobHistoryGuid },
                    Value = ToMultiObjectValue(jobHistoryId)
                }
            };
            return fieldRefValuePairs;
        }

        private static IEnumerable<RelativityObjectRef> ToMultiObjectValue(params int[] artifactIds)
        {
            return artifactIds.Select(x => new RelativityObjectRef { ArtifactID = x });
        }
    }
}
