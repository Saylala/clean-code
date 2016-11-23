using System;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class RendererTests
    {
        private Renderer renderer;

        [SetUp]
        public void SetUp()
        {
            var language = new Markdown();
            renderer = new Renderer(language);
        }

        [Test]
        public void MdRenderer_EmptyStringGiven_EmptyStringReturned()
        {
            var input = string.Empty;

            var result = renderer.Render(input);

            Assert.AreEqual(string.Empty, result);
        }

        [TestCase("abc")]
        [TestCase("123")]
        [TestCase(" ")]
        [TestCase("abc123")]
        [TestCase("123 abc")]
        public void MdRenderer_StringWithoutTagsGiven_InputStringReturned(string input)
        {
            var result = renderer.Render(input);

            Assert.AreEqual(input, result);
        }

        [TestCase("_ab_c", ExpectedResult = "<em>ab</em>c")]
        [TestCase("_a_", ExpectedResult = "<em>a</em>")]
        [TestCase("abc_d_", ExpectedResult = "abc<em>d</em>")]
        [TestCase("xy_zx_y", ExpectedResult = "xy<em>zx</em>y")]
        public string MdRenderer_StringWithItalicGiven_ItalicTagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase(@"\__ab\__c\_abc\_", ExpectedResult = "__ab__c_abc_")]
        [TestCase(@"\__d\__ \_b\_ ", ExpectedResult = "__d__ _b_ ")]
        [TestCase(@"\__yzx\__ \_yzx\_ \__yzx\__", ExpectedResult = "__yzx__ _yzx_ __yzx__")]
        [TestCase(@"abc\__d\__", ExpectedResult = "abc__d__")]
        [TestCase(@"\_ab\_c", ExpectedResult = "_ab_c")]
        public string MdRenderer_StringWithEscapedTags_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab__c", ExpectedResult = "<strong>ab</strong>c")]
        [TestCase("__a__", ExpectedResult = "<strong>a</strong>")]
        [TestCase("abc__d__", ExpectedResult = "abc<strong>d</strong>")]
        [TestCase("xy__zx__y", ExpectedResult = "xy<strong>zx</strong>y")]
        public string MdRenderer_StringWithBoldGiven_BoldTagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab_abc_ab__", ExpectedResult = "<strong>ab<em>abc</em>ab</strong>")]
        [TestCase("__a _b_ _b_ _b_ c__", ExpectedResult = "<strong>a <em>b</em> <em>b</em> <em>b</em> c</strong>")]
        [TestCase("__abc_abc_abc__ ", ExpectedResult = "<strong>abc<em>abc</em>abc</strong> ")]
        [TestCase("__y z x_y z x_y z x__", ExpectedResult = "<strong>y z x<em>y z x</em>y z x</strong>")]
        public string MdRenderer_StringWithItalicInsideBoldGiven_AllTagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("_ab__abc__ab_", ExpectedResult = "<em>ab__abc__ab</em>")]
        [TestCase("_a__b__c_", ExpectedResult = "<em>a__b__c</em>")]
        [TestCase("_abc__abc__abc_ ", ExpectedResult = "<em>abc__abc__abc</em> ")]
        [TestCase("_y z x__y z x__y z x_", ExpectedResult = "<em>y z x__y z x__y z x</em>")]
        [TestCase("_y z x__y z x__y z x", ExpectedResult = "_y z x<strong>y z x</strong>y z x")]
        public string MdRenderer_StringWithBoldInsideItalicGiven_BoldTagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__1__", ExpectedResult = "__1__")]
        [TestCase("xy__45__y", ExpectedResult = "xy__45__y")]
        [TestCase("_6_", ExpectedResult = "_6_")]
        [TestCase("xy_78_y", ExpectedResult = "xy_78_y")]
        [TestCase("__12__c_abc_", ExpectedResult = "__12__c<em>abc</em>")]
        [TestCase("0__1__abc", ExpectedResult = "0__1__abc")]
        public string MdRenderer_StringWithTagsAroundDigitsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab_c", ExpectedResult = "__ab_c")]
        [TestCase("__x_", ExpectedResult = "__x_")]
        [TestCase("abc__d_", ExpectedResult = "abc__d_")]
        [TestCase("mn__kj_l", ExpectedResult = "mn__kj_l")]
        public string MdRenderer_StringWithUnpairedTagsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ tags__c", ExpectedResult = "__ tags__c")]
        [TestCase("_ tags_", ExpectedResult = "_ tags_")]
        [TestCase("abc_ d_", ExpectedResult = "abc_ d_")]
        [TestCase("mn__ abc__l", ExpectedResult = "mn__ abc__l")]
        public string MdRenderer_StringWithInvalidOpeningTagsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__tags __c", ExpectedResult = "__tags __c")]
        [TestCase("_tags _", ExpectedResult = "_tags _")]
        [TestCase("abc_d _", ExpectedResult = "abc_d _")]
        [TestCase("mn__abc __l", ExpectedResult = "mn__abc __l")]
        public string MdRenderer_StringWithInvalidClosingTagsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab__c_abc_", ExpectedResult = "<strong>ab</strong>c<em>abc</em>")]
        [TestCase("__d__ _b_ ", ExpectedResult = "<strong>d</strong> <em>b</em> ")]
        [TestCase("__yzx__ _yzx_ __yzx__", ExpectedResult = "<strong>yzx</strong> <em>yzx</em> <strong>yzx</strong>")]
        public string MdRenderer_StringWithMultipleTagsGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase(100, 10000, "1")]
        [TestCase(100, 10000, "__")]
        [TestCase(100, 10000, "_a_")]
        [TestCase(100, 10000, "__a__")]
        public void MdRenderer_ShouldHave_LinearPerformance(int count1, int count2, string pattern)
        {
            const int expectedCoef = 3;

            var text1 = CreateText(pattern, count1);
            var text2 = CreateText(pattern, count2);
            var refTime1 = MeasureReferenceTime(text1);
            var refTime2 = MeasureReferenceTime(text2);

            var time1 = MeasureWorkTime(text1);
            var coef1 = time1 / (double)refTime1;

            var time2 = MeasureWorkTime(text2);
            var coef2 = time2 / (double)refTime2;

            var result = Math.Max(coef1, coef2) / Math.Min(coef1, coef2);

            Assert.Less(result, expectedCoef);
        }

        [TestCase("[Test](http://test.test/)_a_", ExpectedResult = "<a href=\"http://test.test/\">Test</a><em>a</em>")]
        [TestCase("[]()", ExpectedResult = "<a href=\"\"></a>")]
        [TestCase("[1](1)", ExpectedResult = "<a href=\"1\">1</a>")]
        [TestCase("[Test](http://2.2/)", ExpectedResult = "<a href=\"http://2.2/\">Test</a>")]
        public string MdRenderer_StringWithLinksGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("[Test](test)_a_", "http://test.test/", ExpectedResult = "<a href=\"http://test.test/test\">Test</a><em>a</em>")]
        [TestCase("[]()", "http://test.test/", ExpectedResult = "<a href=\"http://test.test/\"></a>")]
        [TestCase("[1](1)", "test", ExpectedResult = "<a href=\"test1\">1</a>")]
        [TestCase("[Test](2)", "http://2.2/", ExpectedResult = "<a href=\"http://2.2/2\">Test</a>")]
        public string MdRenderer_StringWithRelativeLinksGiven_TagsAreReplaced(string input, string baseUrl)
        {
            var markdown = new Markdown(baseUrl: baseUrl);
            var testRenderer = new Renderer(markdown);

            return testRenderer.Render(input);
        }

        [TestCase("_ab_c", "a", ExpectedResult = "<em style=\"a\">ab</em>c")]
        [TestCase("__ab__c", "b", ExpectedResult = "<strong style=\"b\">ab</strong>c")]
        [TestCase("__ab__c_abc_", "c", ExpectedResult = "<strong style=\"c\">ab</strong>c<em style=\"c\">abc</em>")]
        [TestCase("[]()", "d", ExpectedResult = "<a href=\"\" style=\"d\"></a>")]
        public string MdRenderer_StyleSettedGiven_TagsAreReplacedWithStyle(string input, string style)
        {
            var markdown = new Markdown(style);
            var testRenderer = new Renderer(markdown);

            return testRenderer.Render(input);
        }

        [TestCase("a", ExpectedResult = "<p>a</p>")]
        [TestCase("a\n\na", ExpectedResult = "<p>a</p><p>a</p>")]
        [TestCase("\na\n", ExpectedResult = "<p>a</p>")]
        [TestCase("\n\na\n\n", ExpectedResult = "<p>a</p>")]
        public string MdRenderer_StringWithParagraphsGiven_TagsAreReplaced(string input)
        {
            var markdown = new Markdown(paragraphsEnabled: true);
            var testRenderer = new Renderer(markdown);

            return testRenderer.Render(input);
        }

        [TestCase("#a#", ExpectedResult = "<h1>a</h1>")]
        [TestCase("#a# #a#", ExpectedResult = "<h1>a</h1> <h1>a</h1>")]
        [TestCase("#a# #a\n", ExpectedResult = "<h1>a</h1> <h1>a</h1>")]
        [TestCase("##a## #a#", ExpectedResult = "<h2>a</h2> <h1>a</h1>")]
        [TestCase("#a# ##a##", ExpectedResult = "<h1>a</h1> <h2>a</h2>")]
        [TestCase("##a\n", ExpectedResult = "<h2>a</h2>")]
        [TestCase("##a#", ExpectedResult = "<h2>a</h2>")]
        [TestCase("#####a#", ExpectedResult = "<h5>a</h5>")]
        [TestCase("#a##", ExpectedResult = "<h1>a</h1>")]
        public string MdRenderer_StringWithHeadersGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("    a", ExpectedResult = "<pre><code>a</code></pre>")]
        [TestCase("        a", ExpectedResult = "<pre><code>    a</code></pre>")]
        [TestCase("    a\n    a", ExpectedResult = "<pre><code>a\na</code></pre>")]
        [TestCase("    a\n        a", ExpectedResult = "<pre><code>a\n    a</code></pre>")]
        public string MdRenderer_StringWithCodeBlocksGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("1. 1", ExpectedResult = "<ol><li>1</li></ol>")]
        [TestCase("1. 1\n2. 2", ExpectedResult = "<ol><li>1\n</li><li>2</li></ol>")]
        [TestCase("2. 1\n1. 2", ExpectedResult = "<ol><li>1\n</li><li>2</li></ol>")]
        [TestCase("2. 1\n2. 2", ExpectedResult = "<ol><li>1\n</li><li>2</li></ol>")]
        public string MdRenderer_StringWithListGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        private static string CreateText(string pattern, double length)
        {
            var builder = new StringBuilder();
            while (builder.Length < length)
                builder.Append(pattern);
            return builder.ToString();
        }

        private long MeasureWorkTime(string text)
        {
            var sw = new Stopwatch();

            // warm up
            renderer.Render(text);
            renderer.Render(text);
            renderer.Render(text);

            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            sw.Start();
            renderer.Render(text);
            sw.Stop();

            return sw.ElapsedTicks;
        }

        private static long MeasureReferenceTime(string text)
        {
            var sw = new Stopwatch();

            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            sw.Start();
            foreach (var symbol in text)
            {
                var hash = symbol.GetHashCode();
                var test = hash.GetHashCode();
            }
            sw.Stop();

            return sw.ElapsedTicks;
        }
    }
}
