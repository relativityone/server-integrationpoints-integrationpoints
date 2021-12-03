using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeFieldsRepository : IFieldsRepository
    {
        public List<Tuple<int, IEnumerable<RelativityObject>>> WorkspacesFields { get; set; }

        public Task<IEnumerable<DocumentFieldInfo>> GetAllDocumentFieldsAsync(int workspaceId)
        {
            ValidateWorkspacesDocumentFields();

            IEnumerable<DocumentFieldInfo> documentFields = GetDocumentFieldsAsync(WorkspacesFields, workspaceId);

            return Task.FromResult(documentFields);
        }

        public Task<IEnumerable<DocumentFieldInfo>> GetFieldsByArtifactsIdAsync(IEnumerable<string> artifactIds, int workspaceId)
        {
            ValidateWorkspacesDocumentFields();
            
            IEnumerable<Tuple<int, IEnumerable<RelativityObject>>> workspacesFields =
                WorkspacesFields.Select(x => 
                    new Tuple<int, IEnumerable<RelativityObject>>(x.Item1, x.Item2.Where( y => artifactIds
                        .Contains(y.ArtifactID.ToString()))));

            IEnumerable<DocumentFieldInfo> documentFields = GetDocumentFieldsAsync(workspacesFields.ToList(), workspaceId);

            return Task.FromResult(documentFields);
        }

        private void ValidateWorkspacesDocumentFields()
        {
            if (WorkspacesFields.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(WorkspacesFields), $@"Please set {nameof(WorkspacesFields)} property first");
            }
        }

        private IEnumerable<DocumentFieldInfo> GetDocumentFieldsAsync(
            List<Tuple<int, IEnumerable<RelativityObject>>> workspacesFields, int workspaceId)
        {
            IEnumerable<Tuple<int, IEnumerable<DocumentFieldInfo>>> workspacesDocumentFieldsInfo = WorkspacesFieldsToWorkspacesDocumentFieldInfo(workspacesFields);

            IEnumerable<DocumentFieldInfo> documentFields = workspacesDocumentFieldsInfo.Single(x => x.Item1 == workspaceId).Item2;

            return documentFields;
        }

        private IEnumerable<Tuple<int, IEnumerable<DocumentFieldInfo>>> WorkspacesFieldsToWorkspacesDocumentFieldInfo(
            List<Tuple<int, IEnumerable<RelativityObject>>> workspacesFields)
        {
            IEnumerable<Tuple<int, IEnumerable<DocumentFieldInfo>>> workspacesDocumentFieldsInfo;

            workspacesDocumentFieldsInfo = workspacesFields.Select(x => new Tuple<int, IEnumerable<DocumentFieldInfo>>(x.Item1, x.Item2.Select(FieldConvert.ToDocumentFieldInfo)));

            return workspacesDocumentFieldsInfo;
        }
    }
}
