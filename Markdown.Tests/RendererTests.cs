using System;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class RendererTests
    {
        private MdRenderer renderer;

        [SetUp]
        public void SetUp()
        {
            var language = new Markdown();
            renderer = new MdRenderer(language);
        }

        [TestCase("abc", ExpectedResult = "<html><p>abc</p></html>")]
        [TestCase("123", ExpectedResult = "<html><p>123</p></html>")]
        [TestCase(" ", ExpectedResult = "<html><p> </p></html>")]
        [TestCase("abc123", ExpectedResult = "<html><p>abc123</p></html>")]
        [TestCase("123 abc", ExpectedResult = "<html><p>123 abc</p></html>")]
        public string MdRenderer_StringWithoutTagsGiven_InputStringWithTagsReturned(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("_ab_c", ExpectedResult = "<html><p><em>ab</em>c</p></html>")]
        [TestCase("_a_", ExpectedResult = "<html><p><em>a</em></p></html>")]
        [TestCase("abc_d_", ExpectedResult = "<html><p>abc<em>d</em></p></html>")]
        [TestCase("xy_zx_y", ExpectedResult = "<html><p>xy<em>zx</em>y</p></html>")]
        public string MdRenderer_StringWithItalicGiven_ItalicTagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase(@"\__ab\__c\_abc\_", ExpectedResult = "<html><p>__ab__c_abc_</p></html>")]
        [TestCase(@"\__d\__ \_b\_ ", ExpectedResult = "<html><p>__d__ _b_ </p></html>")]
        [TestCase(@"\__yzx\__ \_yzx\_ \__yzx\__", ExpectedResult = "<html><p>__yzx__ _yzx_ __yzx__</p></html>")]
        [TestCase(@"abc\__d\__", ExpectedResult = "<html><p>abc__d__</p></html>")]
        [TestCase(@"\_ab\_c", ExpectedResult = "<html><p>_ab_c</p></html>")]
        public string MdRenderer_StringWithEscapedTags_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab__c", ExpectedResult = "<html><p><strong>ab</strong>c</p></html>")]
        [TestCase("__a__", ExpectedResult = "<html><p><strong>a</strong></p></html>")]
        [TestCase("abc__d__", ExpectedResult = "<html><p>abc<strong>d</strong></p></html>")]
        [TestCase("xy__zx__y", ExpectedResult = "<html><p>xy<strong>zx</strong>y</p></html>")]
        public string MdRenderer_StringWithBoldGiven_BoldTagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab_abc_ab__", ExpectedResult = "<html><p><strong>ab<em>abc</em>ab</strong></p></html>")]
        [TestCase("__a _b_ _b_ _b_ c__", ExpectedResult = "<html><p><strong>a <em>b</em> <em>b</em> <em>b</em> c</strong></p></html>")]
        [TestCase("__abc_abc_abc__ ", ExpectedResult = "<html><p><strong>abc<em>abc</em>abc</strong> </p></html>")]
        [TestCase("__y z x_y z x_y z x__", ExpectedResult = "<html><p><strong>y z x<em>y z x</em>y z x</strong></p></html>")]
        public string MdRenderer_StringWithItalicInsideBoldGiven_AllTagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("_ab__abc__ab_", ExpectedResult = "<html><p><em>ab__abc__ab</em></p></html>")]
        [TestCase("_a__b__c_", ExpectedResult = "<html><p><em>a__b__c</em></p></html>")]
        [TestCase("_abc__abc__abc_ ", ExpectedResult = "<html><p><em>abc__abc__abc</em> </p></html>")]
        [TestCase("_y z x__y z x__y z x_", ExpectedResult = "<html><p><em>y z x__y z x__y z x</em></p></html>")]
        [TestCase("_y z x__y z x__y z x", ExpectedResult = "<html><p>_y z x<strong>y z x</strong>y z x</p></html>")]
        public string MdRenderer_StringWithBoldInsideItalicGiven_BoldTagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__1__", ExpectedResult = "<html><p>__1__</p></html>")]
        [TestCase("xy__45__y", ExpectedResult = "<html><p>xy__45__y</p></html>")]
        [TestCase("_6_", ExpectedResult = "<html><p>_6_</p></html>")]
        [TestCase("xy_78_y", ExpectedResult = "<html><p>xy_78_y</p></html>")]
        [TestCase("__12__c_abc_", ExpectedResult = "<html><p>__12__c<em>abc</em></p></html>")]
        [TestCase("0__1__abc", ExpectedResult = "<html><p>0__1__abc</p></html>")]
        public string MdRenderer_StringWithTagsAroundDigitsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab_c", ExpectedResult = "<html><p>__ab_c</p></html>")]
        [TestCase("__x_", ExpectedResult = "<html><p>__x_</p></html>")]
        [TestCase("abc__d_", ExpectedResult = "<html><p>abc__d_</p></html>")]
        [TestCase("mn__kj_l", ExpectedResult = "<html><p>mn__kj_l</p></html>")]
        public string MdRenderer_StringWithUnpairedTagsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ tags__c", ExpectedResult = "<html><p>__ tags__c</p></html>")]
        [TestCase("_ tags_", ExpectedResult = "<html><p>_ tags_</p></html>")]
        [TestCase("abc_ d_", ExpectedResult = "<html><p>abc_ d_</p></html>")]
        [TestCase("mn__ abc__l", ExpectedResult = "<html><p>mn__ abc__l</p></html>")]
        public string MdRenderer_StringWithInvalidOpeningTagsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__tags __c", ExpectedResult = "<html><p>__tags __c</p></html>")]
        [TestCase("_tags _", ExpectedResult = "<html><p>_tags _</p></html>")]
        [TestCase("abc_d _", ExpectedResult = "<html><p>abc_d _</p></html>")]
        [TestCase("mn__abc __l", ExpectedResult = "<html><p>mn__abc __l</p></html>")]
        public string MdRenderer_StringWithInvalidClosingTagsGiven_TagsAreNotReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("__ab__c_abc_", ExpectedResult = "<html><p><strong>ab</strong>c<em>abc</em></p></html>")]
        [TestCase("__d__ _b_ ", ExpectedResult = "<html><p><strong>d</strong> <em>b</em> </p></html>")]
        [TestCase("__yzx__ _yzx_ __yzx__", ExpectedResult = "<html><p><strong>yzx</strong> <em>yzx</em> <strong>yzx</strong></p></html>")]
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

        [TestCase("[Test](http://test.test/)_a_", ExpectedResult = "<html><p><a href=\"http://test.test/\">Test</a><em>a</em></p></html>")]
        [TestCase("[]()", ExpectedResult = "<html><p><a href=\"\"></a></p></html>")]
        [TestCase("[1](1)", ExpectedResult = "<html><p><a href=\"1\">1</a></p></html>")]
        [TestCase("[Test](http://2.2/)", ExpectedResult = "<html><p><a href=\"http://2.2/\">Test</a></p></html>")]
        public string MdRenderer_StringWithLinksGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("[Test](test)_a_", "http://test.test/", ExpectedResult = "<html><p><a href=\"http://test.test/test\">Test</a><em>a</em></p></html>")]
        [TestCase("[]()", "http://test.test/", ExpectedResult = "<html><p><a href=\"http://test.test/\"></a></p></html>")]
        [TestCase("[1](1)", "test", ExpectedResult = "<html><p><a href=\"test1\">1</a></p></html>")]
        [TestCase("[Test](2)", "http://2.2/", ExpectedResult = "<html><p><a href=\"http://2.2/2\">Test</a></p></html>")]
        public string MdRenderer_StringWithRelativeLinksGiven_TagsAreReplaced(string input, string baseUrl)
        {
            var markdown = new Markdown(baseUrl: baseUrl);
            var testRenderer = new MdRenderer(markdown);

            return testRenderer.Render(input);
        }

        [TestCase("_ab_c", "a", ExpectedResult = "<html style=\"a\"><p style=\"a\"><em style=\"a\">ab</em>c</p></html>")]
        [TestCase("__ab__c", "b", ExpectedResult = "<html style=\"b\"><p style=\"b\"><strong style=\"b\">ab</strong>c</p></html>")]
        [TestCase("__ab__c_abc_", "c", ExpectedResult = "<html style=\"c\"><p style=\"c\"><strong style=\"c\">ab</strong>c<em style=\"c\">abc</em></p></html>")]
        [TestCase("[]()", "d", ExpectedResult = "<html style=\"d\"><p style=\"d\"><a href=\"\" style=\"d\"></a></p></html>")]
        public string MdRenderer_StyleSettedGiven_TagsAreReplacedWithStyle(string input, string style)
        {
            var markdown = new Markdown(style);
            var testRenderer = new MdRenderer(markdown);

            return testRenderer.Render(input);
        }

        [TestCase("a", ExpectedResult = "<html><p>a</p></html>")]
        [TestCase("a\n\na", ExpectedResult = "<html><p>a</p><p>a</p></html>")]
        [TestCase("\na\n", ExpectedResult = "<html><p>a</p></html>")]
        [TestCase("\n\na\n\n", ExpectedResult = "<html><p>a</p></html>")]
        public string MdRenderer_StringWithParagraphsGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("#a#", ExpectedResult = "<html><p><h1>a</h1></p></html>")]
        [TestCase("#a# #a#", ExpectedResult = "<html><p><h1>a</h1> <h1>a</h1></p></html>")]
        [TestCase("#a# #a\n", ExpectedResult = "<html><p><h1>a</h1> <h1>a</h1></p></html>")]
        [TestCase("##a## #a#", ExpectedResult = "<html><p><h2>a</h2> <h1>a</h1></p></html>")]
        [TestCase("#a# ##a##", ExpectedResult = "<html><p><h1>a</h1> <h2>a</h2></p></html>")]
        [TestCase("##a\n", ExpectedResult = "<html><p><h2>a</h2></p></html>")]
        [TestCase("##a#", ExpectedResult = "<html><p><h2>a</h2></p></html>")]
        [TestCase("#####a#", ExpectedResult = "<html><p><h5>a</h5></p></html>")]
        [TestCase("#a##", ExpectedResult = "<html><p><h1>a</h1></p></html>")]
        public string MdRenderer_StringWithHeadersGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("    a", ExpectedResult = "<html><p><pre><code>a</code></pre></p></html>")]
        [TestCase("        a", ExpectedResult = "<html><p><pre><code>    a</code></pre></p></html>")]
        [TestCase("    a\n    a", ExpectedResult = "<html><p><pre><code>a\na</code></pre></p></html>")]
        [TestCase("    a\n        a", ExpectedResult = "<html><p><pre><code>a\n    a</code></pre></p></html>")]
        public string MdRenderer_StringWithCodeBlocksGiven_TagsAreReplaced(string input)
        {
            return renderer.Render(input);
        }

        [TestCase("1. 1", ExpectedResult = "<html><p><ol><li>1</li></ol></p></html>")]
        [TestCase("1. 1\n2. 2", ExpectedResult = "<html><p><ol><li>1\n</li><li>2</li></ol></p></html>")]
        [TestCase("2. 1\n1. 2", ExpectedResult = "<html><p><ol><li>1\n</li><li>2</li></ol></p></html>")]
        [TestCase("2. 1\n2. 2", ExpectedResult = "<html><p><ol><li>1\n</li><li>2</li></ol></p></html>")]
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
