using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Markdown
{
    public class Markdown : IMarkupLanguage
    {
        private string htmlUrlTag;
        private readonly string baseUrl;

        private readonly HashSet<string> tags = new HashSet<string>
        {
            "_",
            "__",
            "[",
            "]",
            "\n",
            "#",
            "##",
            "###",
            "####",
            "#####",
            "######"
        };

        private readonly Dictionary<string, string> tagPairs = new Dictionary<string, string>
        {
            {"_", "_"},
            {"__", "__"},
            {"[", "]" }
        };

        public ImmutableDictionary<string, string> OpeningHtmlTags { get; set; }

        public ImmutableDictionary<string, string> ClosingHtmlTags { get; } = new Dictionary<string, string>
        {
            { "_", "</em>" },
            { "__", "</strong>" },
            { "\n", "</p>" },
            {"#", "</h1>" },
            {"##", "</h2>" },
            {"###", "</h3>" },
            {"####", "</h4>" },
            {"#####", "</h5>" },
            {"######", "</h6>" }
        }.ToImmutableDictionary();

        public bool ParagraphsEnabled { get; }
        public Tuple<string, string> CodeBlockTags { get; set; }
        public Tuple<string, string> ListTags { get; set; }
        public Tuple<string, string> ListEntryTag { get; set; }

        public Markdown(string style = null, string baseUrl = null, bool paragraphsEnabled = false)
        {
            this.baseUrl = baseUrl;
            ParagraphsEnabled = paragraphsEnabled;

            var styleString = style == null ? "" : $@" style=""{style}""";
            SetStyle(styleString);
        }

        public Tag GetTagFromString(string tag, int position)
        {
            return new Tag(tag, position);
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag) || IsBeginningOfList(tag) || IsBeginningOfCodeBlock(tag);
        }

        public bool ArePairedTags(Tag opening, Tag closing)
        {
            if ((closing.Representation == "\n" || closing.Representation[0] == '#') && opening.Representation[0] == '#')
                return true;
            return closing.Representation == tagPairs[opening.Representation];
        }

        public bool IsValidTagContents(char symbol)
        {
            return !char.IsDigit(symbol);
        }

        public bool IsContentRestrictied(Tag tag)
        {
            return tag.Representation == "_" || tag.Representation == "__";
        }

        public bool IsTagWithValidSurroundings(string text, Tag tag, bool isOpeningTag)
        {
            var bias = isOpeningTag ? tag.Representation.Length : -1;
            if (tag.Position + bias >= text.Length || tag.Position + bias < 0)
                return true;
            return text[tag.Position + bias] != ' ';
        }

        public bool IsEscapedTag(Tag tag, string text)
        {
            if (tag.Position == 0)
                return false;
            return text[tag.Position - 1] == '\\';
        }

        public bool CanTagBeNestedInside(Tag tag, Tag other)
        {
            return tag.Representation == "_" && other.Representation == "__";
        }

        public string WrapInHtmlUrlTag(string title, string url)
        {
            var resultUrl = baseUrl == null ? url : baseUrl + url;
            return string.Format(htmlUrlTag, title, resultUrl);
        }

        public bool IsBeginningOfCodeBlock(string tag)
        {
            return (tag.Length == 1 && tag[0] == '\t') || (tag.Length == 4 && tag.All(x => x == ' '));
        }

        public bool IsBeginningOfList(string tag)
        {
            if (tag.Length < 3 || tag.Substring(tag.Length - 2) != ". ")
                return false;
            var isNumber = tag.Substring(0, tag.Length - 2).All(char.IsDigit);
            return isNumber;
        }

        private void SetStyle(string style)
        {
            var openingTags = new Dictionary<string, string>
            {
                {"_", "<em{0}>"},
                {"__", "<strong{0}>"},
                {"\n", "<p{0}>"},
                { "#", "<h1{0}>" },
                {"##", "<h2{0}>" },
                { "###", "<h3{0}>" },
                { "####", "<h4{0}>" },
                { "#####", "<h5{0}>" },
                { "######", "<h6{0}>" }
            };

            OpeningHtmlTags = openingTags
                .ToDictionary(x => x.Key, x => string.Format(x.Value, style))
                .ToImmutableDictionary();

            htmlUrlTag = $"<a href=\"{{1}}\"{style}>{{0}}</a>";

            CodeBlockTags = Tuple.Create($"<pre{style}><code{style}>", "</code></pre>");
            ListTags = Tuple.Create($"<ol{style}>", "</ol>");
            ListEntryTag = Tuple.Create($"<li{style}>", "</li>");
        }
    }
}
