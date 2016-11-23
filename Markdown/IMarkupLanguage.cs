using System;
using System.Collections.Immutable;

namespace Markdown
{
    public interface IMarkupLanguage
    {
        ImmutableDictionary<string, string> OpeningHtmlTags { get; }
        ImmutableDictionary<string, string> ClosingHtmlTags { get; }
        bool ParagraphsEnabled { get; }
        Tuple<string, string> CodeBlockTags { get; }
        Tuple<string, string> ListTags { get; }
        Tuple<string, string> ListEntryTag { get; }
        Tag GetTagFromString(string tag, int position);
        bool HasTag(string tag);
        bool ArePairedTags(Tag opening, Tag closing);
        bool IsValidTagContents(char symbol);
        bool IsContentRestrictied(Tag tag);
        bool IsTagWithValidSurroundings(string text, Tag tag, bool isOpeningTag);
        bool IsEscapedTag(Tag tag, string text);
        bool CanTagBeNestedInside(Tag tag, Tag other);
        string WrapInHtmlUrlTag(string title, string url);
        bool IsBeginningOfCodeBlock(string tag);
        bool IsBeginningOfList(string tag);
    }
}
