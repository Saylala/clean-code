using System.Collections.Generic;
using System.Collections.Immutable;

namespace Markdown
{
    public class Markdown : IMarkupLanguage
    {
        private readonly HashSet<string> tags = new HashSet<string>
        {
            "_",
            "__",
            "[",
            "]"
        };

        private readonly Dictionary<string, string> tagPairs = new Dictionary<string, string>
        {
            {"_", "_"},
            {"__", "__"},
            {"[", "]" }
        };

        public ImmutableDictionary<string, string> OpeningHtmlTags { get; } = new Dictionary<string, string>
        {
            { "_", "<em>" },
            { "__", "<strong>" }
        }.ToImmutableDictionary();

        public ImmutableDictionary<string, string> ClosingHtmlTags { get; } = new Dictionary<string, string>
        {
            { "_", "</em>" },
            { "__", "</strong>" }
        }.ToImmutableDictionary();

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
    }
}
