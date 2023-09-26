using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    /// <inheritdoc />
    public class SavedSearchFake : RdoFakeBase
    {
        public string Name { get; set; }

        public string Owner { get; set; }

        public SearchCriteria SearchCriteria { get; set; }

        public SavedSearchFake() : base("SavedSearch")
        {
            SearchCriteria = new SearchCriteria();
        }

        public SavedSearchFake(SearchCriteria searchCriteria) : base("SavedSearch")
        {
            SearchCriteria = searchCriteria;
        }

        public override List<Guid> Guids => new List<Guid>();

        public override RelativityObject ToRelativityObject()
        {
            return new RelativityObject
            {
                ArtifactID = ArtifactId,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = ParentObjectArtifactId
                },
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair
                    {
                        Field = new Field
                        {
                            Name = SavedSearchFieldsConstants.NAME_FIELD,
                        },
                        Value = Name
                    },
                    new FieldValuePair
                    {
                        Field = new Field
                        {
                            Name = SavedSearchFieldsConstants.OWNER_FIELD,
                        },
                        Value = Owner
                    },
                },
            };
        }
    }
}
