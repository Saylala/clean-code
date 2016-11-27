using NUnit.Framework;

namespace Markdown.Tests
{
    [TestFixture]
    class MarkdownTests
    {
        private Markdown markdown;

        [SetUp]
        public void SetUp()
        {
            markdown = new Markdown();
        }

        [TestCase("#")]
        [TestCase("##")]
        [TestCase("###")]
        [TestCase("####")]
        [TestCase("#####")]
        [TestCase("######")]
        public void IsHeaderTag_ForCorrectHeader_ReturnsTrue(string text)
        {
            Assert.IsTrue(markdown.IsHeaderTag(text));
        }

        [TestCase("test")]
        [TestCase(" ")]
        [TestCase("########")]
        public void IsHeaderTag_ForInvalidHeader_ReturnsFalse(string text)
        {
            Assert.IsFalse(markdown.IsHeaderTag(text));
        }

        [TestCase("    ")]
        [TestCase("\t")]
        public void IsBlockStart_ForCorrectBlockStarter_ReturnsTrue(string text)
        {
            Assert.IsTrue(markdown.IsBeginningOfCodeBlock(text));
        }

        [TestCase("test")]
        [TestCase("1")]
        [TestCase("########")]
        public void IsBlockStart_ForInvalidBlockStarter_ReturnsFalse(string text)
        {
            Assert.IsFalse(markdown.IsBeginningOfCodeBlock(text));
        }

        [TestCase("1. ")]
        [TestCase("2. ")]
        [TestCase("12. ")]
        [TestCase("21. ")]
        [TestCase("2111111111111111111. ")]
        public void IsListStart_ForCorrectListStarter_ReturnsTrue(string text)
        {
            Assert.IsTrue(markdown.IsBeginningOfList(text));
        }

        [TestCase("1.")]
        [TestCase(" 2.")]
        [TestCase("1a2. ")]
        [TestCase("a21. ")]
        [TestCase("aa. ")]
        public void IsListStart_ForInvalidListStarter_ReturnsFalse(string text)
        {
            Assert.IsFalse(markdown.IsBeginningOfList(text));
        }

        [TestCase("style")]
        [TestCase("test")]
        [TestCase("text")]
        [TestCase(" ")]
        [TestCase("")]
        public void SetStyle_ForStyle_AddsStyleToOpeningTags(string text)
        {
            var style = $@" style=""{text}""";
            markdown.SetStyle(style);

            foreach (string tag in markdown.OpeningHtmlTags.Values)
                Assert.IsTrue(tag.Contains(style));
        }
    }
}
