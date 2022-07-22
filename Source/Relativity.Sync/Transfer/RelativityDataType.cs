using System.ComponentModel;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Values were taken from SQL view which is generating type names based on type id.
    /// </summary>
    internal enum RelativityDataType
    {
        [Description("Fixed-Length Text")]
        FixedLengthText = 0,

        [Description("Whole Number")]
        WholeNumber = 1,
        
        [Description("Date")]
        Date = 2,
        
        [Description("Yes/No")]
        YesNo = 3,
        
        [Description("Long Text")]
        LongText = 4,
        
        [Description("Single Choice")]
        SingleChoice = 5,
        
        [Description("Decimal")]
        Decimal = 6,
        
        [Description("Currency")]
        Currency = 7,
        
        [Description("Multiple Choice")]
        MultipleChoice = 8,
        
        [Description("File")]
        File = 9,
        
        [Description("Single Object")]
        SingleObject = 10,
        
        [Description("User")]
        User = 11,
        
        [Description("Multiple Object")]
        MultipleObject = 13
    }
}