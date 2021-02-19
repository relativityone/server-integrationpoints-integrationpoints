using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.RDOs.Framework.Attributes;

namespace Relativity.Sync.Tests.System.Core
{
    [Rdo("3E4B704D-B5D5-4BD3-B3C1-ADA89F0856ED", nameof(SampleRdo))]
    internal sealed class SampleRdo : IRdoType
    {
        
        [RdoField("F6E1CD6F-70D9-4E98-A79C-F980BD107BC7", RdoFieldType.WholeNumber)]
        public int SomeField { get; set; }
        
        [RdoField("95367659-43EE-49E9-AC76-89D2DF4B453C",RdoFieldType.FixedLengthText, fixedTextLength: 64, required: true)]
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
                    TextLenght = 255,
                    PropertyInfo = typeof(SampleRdo).GetProperties().First(x => x.Name == nameof(SomeField))
                }},
                {new Guid("95367659-43EE-49E9-AC76-89D2DF4B453C"), new RdoFieldInfo
                {
                    Name = nameof(SampleRdo.OptionalTextField),
                    Guid = new Guid("95367659-43EE-49E9-AC76-89D2DF4B453C"),
                    Type = RdoFieldType.FixedLengthText,
                    IsRequired = true,
                    TextLenght = 64,
                    PropertyInfo = typeof(SampleRdo).GetProperties().First(x => x.Name == nameof(OptionalTextField))
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

        [RdoField("D2ADC310-F206-4FFD-AA44-818F48A88BEB", RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid GuidField { get; set; }
        
        [RdoField("BF3649EB-D09C-459E-81CD-2C7887E6DA56", RdoFieldType.FixedLengthText, fixedTextLength: 36)]
        public Guid? NullableGuidField { get; set; }
        
        [RdoField("914DFDD8-A36D-4923-80D4-74BF13BEB2CE", RdoFieldType.LongText)]
        public string LongTextField { get; set; }

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
                    Name = nameof(SomeField),
                    Guid = new Guid("F6E1CD6F-70D9-4E98-A79C-F980BD107BC7"),
                    Type = RdoFieldType.WholeNumber,
                    IsRequired = false,
                    TextLenght = 255,
                    PropertyInfo = typeof(ExtendedSampleRdo).GetProperties().First(x => x.Name == nameof(SomeField))
                }},
                {new Guid("95367659-43EE-49E9-AC76-89D2DF4B453C"), new RdoFieldInfo
                {
                    Name = nameof(OptionalTextField),
                    Guid = new Guid("95367659-43EE-49E9-AC76-89D2DF4B453C"),
                    Type = RdoFieldType.FixedLengthText,
                    IsRequired = true,
                    TextLenght = 64,
                    PropertyInfo = typeof(ExtendedSampleRdo).GetProperties().First(x => x.Name == nameof(OptionalTextField))
                }},
                {new Guid("E44D02A2-9BD1-4BB1-A5D9-281F25666359"), new RdoFieldInfo
                {
                    Name = nameof(AdditionalYesNoField),
                    Guid = new Guid("E44D02A2-9BD1-4BB1-A5D9-281F25666359"),
                    Type = RdoFieldType.YesNo,
                    TextLenght = 64,
                    PropertyInfo = typeof(ExtendedSampleRdo).GetProperties().First(x => x.Name == nameof(AdditionalYesNoField))
                }},
                {new Guid("D2ADC310-F206-4FFD-AA44-818F48A88BEB"), new RdoFieldInfo
                {
                    Name = nameof(GuidField),
                    Guid = new Guid("D2ADC310-F206-4FFD-AA44-818F48A88BEB"),
                    Type = RdoFieldType.FixedLengthText,
                    TextLenght = 36,
                    PropertyInfo = typeof(ExtendedSampleRdo).GetProperties().First(x => x.Name == nameof(GuidField))
                }},
                {new Guid("BF3649EB-D09C-459E-81CD-2C7887E6DA56"), new RdoFieldInfo
                {
                    Name = nameof(NullableGuidField),
                    Guid = new Guid("BF3649EB-D09C-459E-81CD-2C7887E6DA56"),
                    Type = RdoFieldType.FixedLengthText,
                    TextLenght = 36,
                    PropertyInfo = typeof(ExtendedSampleRdo).GetProperties().First(x => x.Name == nameof(NullableGuidField))
                }},
                {new Guid("914DFDD8-A36D-4923-80D4-74BF13BEB2CE"), new RdoFieldInfo
                {
                    Name = nameof(LongTextField),
                    Guid = new Guid("914DFDD8-A36D-4923-80D4-74BF13BEB2CE"),
                    Type = RdoFieldType.LongText,
                    PropertyInfo = typeof(ExtendedSampleRdo).GetProperties().First(x => x.Name == nameof(LongTextField))
                }}
            }
        };
    }
}