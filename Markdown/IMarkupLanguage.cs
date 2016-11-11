namespace Markdown
{
    interface IMarkupLanguage
    {
        Tag GetTagFromString(string tag, int position);
        bool HasTag(string tag);
        bool IsValidTagContents(char symbol);
        bool IsTagWithValidSurroundings(string text, Tag tag, bool isOpeningTag);
        bool IsEscapedTag(Tag tag, string text);
        bool CanTagBeNestedInside(Tag tag, Tag other);
    }
}
