using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Markdown
{
	[TestFixture]
	class MdTests
	{
		private Md mdRenderer;

		[SetUp]
		public void SetUp()
		{
			mdRenderer = new Md();
		}

		[Test]
		public void MdRenderer_EmptyStringGiven_EmptyStringReturned()
		{
			var input = string.Empty;

			var result = mdRenderer.Render(input);

			Assert.AreEqual(string.Empty, result);
		}

		[TestCase("abc")]
		[TestCase("123")]
		[TestCase(" ")]
		[TestCase("abc123")]
		[TestCase("123 abc")]
		public void MdRenderer_StringWithoutTagsGiven_InputStringReturned(string input)
		{
			var result = mdRenderer.Render(input);

			Assert.AreEqual(input, result);
		}

		[TestCase("_ab_c", ExpectedResult = "<em>ab</em>c")]
		[TestCase("_a_", ExpectedResult = "<em>a</em>")]
		[TestCase("abc_d_", ExpectedResult = "abc<em>d</em>")]
		[TestCase("xy_zx_y", ExpectedResult = "xy<em>zx</em>y")]
		public string MdRenderer_StringWithItalicGiven_ItalicTagsAreReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase(@"\__ab\__c\_abc\_", ExpectedResult = "__ab__c_abc_")]
		[TestCase(@"\__d\__ \_b\_ ", ExpectedResult = "__d__ _b_ ")]
		[TestCase(@"\__yzx\__ \_yzx\_ \__yzx\__", ExpectedResult = "__yzx__ _yzx_ __yzx__")]
		[TestCase(@"abc\__d\__", ExpectedResult = "abc__d__")]
		[TestCase(@"\_ab\_c", ExpectedResult = "_ab_c")]
		public string MdRenderer_StringWithEscapedTags_TagsAreNotReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("__ab__c", ExpectedResult = "<strong>ab</strong>c")]
		[TestCase("__a__", ExpectedResult = "<strong>a</strong>")]
		[TestCase("abc__d__", ExpectedResult = "abc<strong>d</strong>")]
		[TestCase("xy__zx__y", ExpectedResult = "xy<strong>zx</strong>y")]
		public string MdRenderer_StringWithBoldGiven_BoldTagsAreReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("__ab_abc_ab__", ExpectedResult = "<strong>ab<em>abc</em>ab</strong>")]
		[TestCase("__a_b_c__", ExpectedResult = "<strong>a<em>b</em>c</strong>")]
		[TestCase("__abc_abc_abc__ ", ExpectedResult = "<strong>abc<em>abc</em>abc</strong> ")]
		[TestCase("__y z x_y z x_y z x__", ExpectedResult = "<strong>y z x<em>y z x</em>y z x</strong>")]
		public string MdRenderer_StringWithItalicInsideBoldGiven_AllTagsAreReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("_ab__abc__ab_", ExpectedResult = "<em>ab__abc__ab</em>")]
		[TestCase("_a__b__c_", ExpectedResult = "<em>a__b__c</em>")]
		[TestCase("_abc__abc__abc_ ", ExpectedResult = "<em>abc__abc__abc</em> ")]
		[TestCase("_y z x__y z x__y z x_", ExpectedResult = "<em>y z x__y z x__y z x</em>")]
		public string MdRenderer_StringWithBoldInsideItalicGiven_BoldTagsAreNotReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("__1__", ExpectedResult = "__1__")]
		[TestCase("xy__45__y", ExpectedResult = "xy__45__y")]
		[TestCase("_6_", ExpectedResult = "_6_")]
		[TestCase("xy_78_y", ExpectedResult = "xy_78_y")]
		[TestCase("__12__c_abc_", ExpectedResult = "__12__c<em>abc</em>")]
		[TestCase("__0__ __abc__ ", ExpectedResult = "__0__ <strong>abc</strong> ")]
		public string MdRenderer_StringWithTagsAroundDigitsGiven_TagsAreNotReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("__ab_c", ExpectedResult = "__ab_c")]
		[TestCase("__x_", ExpectedResult = "__x_")]
		[TestCase("abc__d_", ExpectedResult = "abc__d_")]
		[TestCase("mn__kj_l", ExpectedResult = "mn__kj_l")]
		public string MdRenderer_StringWithUnpairedTagsGiven_TagsAreNotReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("__ tags__c", ExpectedResult = "__ tags__c")]
		[TestCase("_ tags_", ExpectedResult = "_ tags_")]
		[TestCase("abc_ d_", ExpectedResult = "abc_ d_")]
		[TestCase("mn__ abc__l", ExpectedResult = "mn__ abc__l")]
		public string MdRenderer_StringWithInvalidOpeningTagsGiven_TagsAreNotReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("__tags __c", ExpectedResult = "__tags __c")]
		[TestCase("_tags _", ExpectedResult = "_tags _")]
		[TestCase("abc_d _", ExpectedResult = "abc_d _")]
		[TestCase("mn__abc __l", ExpectedResult = "mn__abc __l")]
		public string MdRenderer_StringWithInvalidClosingTagsGiven_TagsAreNotReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}

		[TestCase("__ab__c_abc_", ExpectedResult = "<strong>ab</strong>c<em>abc</em>")]
		//[TestCase("_y___x__", ExpectedResult = "<em>y</em><strong>x</strong>")] behavior???
		[TestCase("__d__ _b_ ", ExpectedResult = "<strong>d</strong> <em>b</em> ")]
		[TestCase("__yzx__ _yzx_ __yzx__", ExpectedResult = "<strong>yzx</strong> <em>yzx</em> <strong>yzx</strong>")]
		public string MdRenderer_StringWithMultipleTagsGiven_TagsAreReplaced(string input)
		{
			var result = mdRenderer.Render(input);

			return result;
		}
		[Test]
		public void Performance_Test()
		{
			var builder = new StringBuilder();
			var test = "_x_ ";
			for (var i = 0; i < 25000; i++)
				builder.Append(test);
			var text = builder.ToString();
			var sw = Stopwatch.StartNew();
			foreach (var symbol in text)
			{
			}
			var linearTime = sw.Elapsed;
			sw.Restart();
			mdRenderer.Render(text);
			var resultTime = sw.Elapsed;
			Assert.AreEqual(linearTime, resultTime);
		}
	}
}
