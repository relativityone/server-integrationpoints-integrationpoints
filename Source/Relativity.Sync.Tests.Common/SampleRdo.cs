using System;
using System.Collections.Generic;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.Tests.System.Core
{
    [Rdo("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED", nameof(SampleRdo))]
    internal sealed class SampleRdo : IRdoType
    {
        
        [RdoField("F6E1CD6F-70D9-4E98-A79C-F980BD107BC7", RdoFieldType.WholeNumber)]
        public int SomeField { get; set; }
        
        [RdoField("95367659-43EE-49E9-AC76-89D2DF4B453C",RdoFieldType.FixedLengthText, fixedTextLength: 64)]
        public string OptionalTextField { get; set; }
        
        public int ArtifactId { get; set; }
        
        internal static RdoTypeInfo ExpectedRdoInfo = new RdoTypeInfo
        {
            Name = nameof(SampleRdo),
            TypeGuid = new Guid("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED"),
            ParentTypeGuid = null,
            Fields = new Dictionary<Guid, RdoFieldInfo>
            {
                {new Guid("F6E1CD6F-70D9-4E98-A79C-F980BD107BC7"), new RdoFieldInfo
                {
                    Name = nameof(SampleRdo.SomeField),
                    Guid = new Guid("F6E1CD6F-70D9-4E98-A79C-F980BD107BC7"),
                    Type = RdoFieldType.WholeNumber,
                    IsRequired = false,
                    TextLenght = 255
                }},
                {new Guid("95367659-43EE-49E9-AC76-89D2DF4B453C"), new RdoFieldInfo
                {
                    Name = nameof(SampleRdo.OptionalTextField),
                    Guid = new Guid("95367659-43EE-49E9-AC76-89D2DF4B453C"),
                    Type = RdoFieldType.FixedLengthText,
                    IsRequired = true,
                    TextLenght = 64
                }}
            }
        };
    }


    [Rdo("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED", nameof(SampleRdo))]
    internal sealed class ExtendedSampleRdo : IRdoType
    {

        [RdoField("F6E1CD6F-70D9-4E98-A79C-F980BD107BC7", RdoFieldType.WholeNumber)]
        public int SomeField { get; set; }

        [RdoField("95367659-43EE-49E9-AC76-89D2DF4B453C", RdoFieldType.FixedLengthText, fixedTextLength: 64)]
        public string OptionalTextField { get; set; }
        
        [RdoField("E44D02A2-9BD1-4BB1-A5D9-281F25666359", RdoFieldType.YesNo)]
        public bool AdditionalYesNoField { get; set; }

        public int ArtifactId { get; set; }
    }
}