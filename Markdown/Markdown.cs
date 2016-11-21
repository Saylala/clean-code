using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Markdown
{
    public class Markdown : IMarkupLanguage
    {
        public Markdown(string style = null, string baseUrl = null, bool paragraphsEnabled = false)
        {
            this.baseUrl = baseUrl;
            ParagraphsEnabled = paragraphsEnabled;

            var styleString = style == null ? "" : $@" style=""{style}""";

            var openingTags = new Dictionary<string, string>
            {
                {"_", "<em{0}>"},
                {"__", "<strong{0}>"},
                {"\n", "<p{0}>"}
            };

            OpeningHtmlTags = openingTags
                .ToDictionary(x => x.Key, x => string.Format(x.Value, styleString))
                .ToImmutableDictionary();

            htmlUrlTag = $"<a href=\"{{1}}\"{styleString}>{{0}}</a>";
        }

        private readonly HashSet<string> tags = new HashSet<string>
        {
            "_",
            "__",
            "[",
            "]",
            "\n"
        };

        private readonly Dictionary<string, string> tagPairs = new Dictionary<string, string>
        {
            {"_", "_"},
            {"__", "__"},
            {"[", "]" }
        };

        public ImmutableDictionary<string, string> OpeningHtmlTags { get; }

        public ImmutableDictionary<string, string> ClosingHtmlTags { get; } = new Dictionary<string, string>
        {
            { "_", "</em>" },
            { "__", "</strong>" },
            { "\n", "</p>" }
        }.ToImmutableDictionary();

        public bool ParagraphsEnabled { get; }

        private readonly string htmlUrlTag;
        private readonly string baseUrl;

        public Tag GetTagFromString(string tag, int position)
        {
            return new Tag(tag, position);
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        public bool ArePairedTags(Tag opening, Tag closing)
        {
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
    }
}
