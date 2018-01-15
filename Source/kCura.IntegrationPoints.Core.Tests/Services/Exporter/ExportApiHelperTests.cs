using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Exporter;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
	[TestFixture]
	public class ExportApiHelperTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			
		}

		private const String MultiObjectParsingError = "Encountered an error while processing multi-object field, this may due to out-of-date version of the software. Please contact administrator for more information.";

		[TestCase("<object>Abc</object>", "Abc")]
		[TestCase("<object>abc</object><object>def</object>", "abc;def")]
		[TestCase(null, null)]
		[TestCase("", "")]
		[TestCase("<object>abc</object><object>def</object><object>abc</object>", "abc;def;abc")]
		public void MultiObjectFieldParsing(object input, object expectedResult)
		{
			object result = ExportApiDataHelper.SanitizeMultiObjectField(input);
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase("NonXmlString", MultiObjectParsingError)]
		[TestCase("<object><object></object>", MultiObjectParsingError)]
		[TestCase("<object></object></object>", MultiObjectParsingError)]
		public void MultiObjectFieldParsing_Exceptions(object input, string expectedExceptionMessage)
		{
			Assert.That(() => ExportApiDataHelper.SanitizeMultiObjectField(input),
				Throws.Exception
					.TypeOf<Exception>()
					.With.Property("Message")
					.EqualTo(expectedExceptionMessage));
		}

		[TestCase("\vChoice1", "Choice1")]
		[TestCase("\vChoice1&#x0B;", "Choice1")]
		[TestCase("\vChoice1&#x0B;&#x0B;&#x0B;&#x0B;", "Choice1")]
		[TestCase("Choice1&#x0B;", "Choice1")]
		[TestCase(null, null)]
		[TestCase("", "")]
		public void SingleChoiceFieldParsing(object input, object expectedResult)
		{
			object result = ExportApiDataHelper.SanitizeSingleChoiceField(input);
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public void RetrieveLongTextFieldAsync_Unicode()
		{
			const string data = @"Each code point has a single General Category property. The major categories are: Letter, Mark, Number, Punctuation, Symbol, Separator and Other. Within these categories, there are subdivisions. The General Category is not useful for every use, since legacy encodings have used multiple characteristics per single code point. E.g., U+000A <control-000A> Line feed (LF) in ASCII is both a control and a formatting separator; in Unicode the General Category is ""Other, Control"". Often, other properties must be used to specify the characteristics and behaviour of a code point. The possible General Categories are:
[show]General Category (Unicode Character Property)[a] v t e
Code points in the range U+D800–U+DBFF (1,024 code points) are known as high-surrogate code points, and code points in the range U+DC00–U+DFFF (1,024 code points) are known as low-surrogate code points. A high-surrogate code point (also known as a leading surrogate) followed by a low-surrogate code point (also known as a trailing surrogate) together form a surrogate pair used in UTF-16 to represent 1,048,576 code points outside BMP. High and low surrogate code points are not valid by themselves. Thus the range of code points that are available for use as characters is U+0000–U+D7FF and U+E000–U+10FFFF (1,112,064 code points). The value of these code points (i.e., excluding surrogates) is sometimes referred to as the character's scalar value.
Certain noncharacter code points are guaranteed never to be used for encoding characters, although applications may make use of these code points internally if they wish. There are sixty-six noncharacters: U+FDD0–U+FDEF and any code point ending in the value FFFE or FFFF (i.e., U+FFFE, U+FFFF, U+1FFFE, U+1FFFF, … U+10FFFE, U+10FFFF). The set of noncharacters is stable, and no new noncharacters will ever be defined.[11]
Reserved code points are those code points which are available for use as encoded characters, but are not yet defined as characters by Unicode.
Private-use code points are considered to be assigned characters, but they have no interpretation specified by the Unicode standard[12] so any interchange of such characters requires an agreement between sender and receiver on their interpretation. There are three private-use areas in the Unicode codespace:
Private Use Area: U+E000–U+F8FF (6,400 characters)
Supplementary Private Use Area-A: U+F0000–U+FFFFD (65,534 characters)
Supplementary Private Use Area-B: U+100000–U+10FFFD (65,534 characters).
Graphic characters are characters defined by Unicode to have a particular semantic, and either have a visible glyph shape or represent a visible space. As of Unicode 8.0 there are 120,520 graphic characters.
Format characters are characters that do not have a visible appearance, but may have an effect on the appearance or behavior of neighboring characters. For example, U+200C ZERO-WIDTH NON-JOINER and U+200D ZERO-WIDTH JOINER may be used to change the default shaping behavior of adjacent characters (e.g., to inhibit ligatures or request ligature formation). There are 152 format characters in Unicode 8.0.
Sixty-five code points (U+0000–U+001F and U+007F–U+009F) are reserved as control codes, and correspond to the C0 and C1 control codes defined in ISO/IEC 6429. Of these U+0009 (Tab), U+000A (Line Feed), and U+000D (Carriage Return) are widely used in Unicode-encoded texts.
Graphic characters, format characters, control code characters, and private use characters are known collectively as assigned characters.
Abstract characters[edit]
The set of graphic and format characters defined by Unicode does not correspond directly to the repertoire of abstract characters that is representable under Unicode. Unicode encodes characters by associating an abstract character with a particular code point.[13] However, not all abstract characters are encoded as a single Unicode character, and some abstract characters may be represented in Unicode by a sequence of two or more characters. For example, a Latin small letter ""i"" with an ogonek, a dot above, and an acute accent, which is required in Lithuanian, is represented by the character sequence U+012F, U+0307, U+0301. Unicode maintains a list of uniquely named character sequences for abstract characters that are not directly encoded in Unicode.[14]
All graphic, format, and private use characters have a unique and immutable name by which they may be identified. This immutability has been guaranteed since Unicode version 2.0 by the Name Stability policy.[11] In cases where the name is seriously defective and misleading, or has a serious typographical error, a formal alias may be defined, and applications are encouraged to use the formal alias in place of the official character name. For example, U+A015 ꀕ YI SYLLABLE WU has the formal alias yi syllable iteration mark, and U+FE18 ︘ PRESENTATION FORM FOR VERTICAL RIGHT WHITE LENTICULAR BRAKCET (sic) has the formal alias presentation form for vertical right white lenticular bracket.[15]
Unicode Consortium[edit]
Main article: Unicode Consortium
The Unicode Consortium is a nonprofit organization that coordinates Unicode's development. Full members include most of the main computer software and hardware companies with any interest in text-processing standards, including Adobe Systems, Apple, Google, IBM, Microsoft, Oracle Corporation, and Yahoo!.[16]
The Consortium has the ambitious goal of eventually replacing existing character encoding schemes with Unicode and its standard Unicode Transformation Format (UTF) schemes, as many of the existing schemes are limited in size and scope and are incompatible with multilingual environments.
Versions[edit]
Unicode is developed in conjunction with the International Organization for Standardization and shares the character repertoire with ISO/IEC 10646: the Universal Character Set. Unicode and ISO/IEC 10646 function equivalently as character encodings, but The Unicode Standard contains much more information for implementers, covering—in depth—topics such as bitwise encoding, collation and rendering. The Unicode Standard enumerates a multitude of character properties, including those needed for supporting bidirectional text. The two standards do use slightly different terminology.
The Consortium first published The Unicode Standard (ISBN 0-321-18578-1) in 1991 and continues to develop standards based on that original work. The latest version of the standard, Unicode 8.0, was released in June 2015 and is available from the consortium's website. The last of the major versions (versions x.0) to be published in book form was Unicode 5.0 (ISBN 0-321-48091-0), but since Unicode 6.0 the full text of the standard is no longer being published in book form. In 2012, however, it was announced that only the core specification for Unicode version 6.1 would be made available as a 692-page print-on-demand paperback.[17] Unlike the previous major version printings of the Standard, the print-on-demand core specification does not include any code charts or standard annexes, but the entire standard, including the core specification, will still remain freely available on the Unicode website.
Thus far the following major and minor versions of the Unicode standard have been published. Update versions, which do not include any changes to character repertoire, are signified by the third number (e.g., ""version 4.0.1"") and are omitted in the table below.[18]";

			InMemoryILongTextStreamFactory factory = new InMemoryILongTextStreamFactory(data, true);
			string result = ExportApiDataHelper.RetrieveLongTextFieldAsync(factory, 1, 2).ConfigureAwait(false).GetAwaiter().GetResult();
			Assert.AreEqual(data, result);
		}
	}
}