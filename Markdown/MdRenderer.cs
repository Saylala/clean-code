using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
    public class MdRenderer
    {
        private int rendered;
        private int position;
        private StringBuilder builder;
        private string text;
        private readonly Markdown language;

        public MdRenderer(Markdown language)
        {
            this.language = language;
        }

        public string Render(string markdownText)
        {
            position = 0;
            rendered = 0;
            text = markdownText;
            builder = new StringBuilder();
            builder.Append(language.BaseTags.OpeningTag);
            while (position < text.Length)
            {
                position = SkipEmtyLines(position);
                if (position >= text.Length)
                    break;
                builder.Append(language.ParagraphTags.OpeningTag);
                RenderNextParagraph(position);
                builder.Append(language.ParagraphTags.ClosingTag);
            }
            builder.Append(language.BaseTags.ClosingTag);
            return builder.ToString();
        }

        private int SkipEmtyLines(int start)
        {
            if (!language.IsLineDelimiter(text[position].ToString()))
                return start;
            var currentLine = start;
            int pos;
            for (pos = start; pos < text.Length && char.IsWhiteSpace(text[pos]); pos++)
            {
                if (language.IsLineDelimiter(text[pos].ToString()))
                    currentLine = pos + 1;
            }
            rendered = currentLine;
            return pos;
        }

        private void RenderNextParagraph(int start)
        {
            foreach (var openingTag in GetTags())
            {
                builder.Append(text.Substring(rendered, openingTag.Position - rendered));
                rendered = openingTag.Position;
                if (!language.IsTagWithValidSurroundings(text, openingTag, true))
                    continue;
                if (language.IsLineDelimiter(openingTag.Name))
                    return;
                RenderNextTagPair(openingTag);
            }
            builder.Append(text.Substring(rendered, text.Length - rendered));
            rendered = text.Length;
        }


        private void RenderNextTagPair(Tag openingTag)
        {
            IEnumerable<Tuple<Tag, Tag>> nestedTags = new List<Tuple<Tag, Tag>>();
            foreach (var tag in GetTags())
            {
                if (language.IsLineDelimiter(tag.Name) && !language.IsHeaderTag(openingTag.Name))
                    return;
                if (language.IsLineDelimiter(tag.Name))
                    position++;

                if (language.ArePairedTags(openingTag, tag))
                {
                    if (!language.IsTagWithValidSurroundings(text, tag, false))
                        continue;
                    if (!language.OpeningHtmlTags.ContainsKey(openingTag.Name))
                    {
                        RenderUrl(openingTag, tag);
                        return;
                    }
                    RenderTags(nestedTags, openingTag, tag, true);
                    return;
                }
                if (language.IsTagWithValidSurroundings(text, tag, true))
                    nestedTags = nestedTags.Concat(GetNestedTags(tag, openingTag)).ToList();
            }
            RenderTags(nestedTags, openingTag, null, false);
        }

        private IEnumerable<Tuple<Tag, Tag>> GetNestedTags(Tag openingTag, Tag surroundingTag)
        {
            foreach (var tag in GetTags())
            {
                if (tag.Name == surroundingTag.Name)
                {
                    if (!language.IsTagWithValidSurroundings(text, tag, false))
                        continue;
                    position -= tag.Name.Length;
                    break;
                }
                if (tag.Name != openingTag.Name) continue;
                yield return Tuple.Create(openingTag, tag);
                break;
            }
        }

        private void RenderTags(IEnumerable<Tuple<Tag, Tag>> tags, Tag openingTag, Tag closingTag, bool isNested)
        {
            var opening = closingTag == null
                ? openingTag.Name
                : language.OpeningHtmlTags[openingTag.Name];
            builder.Append(text.Substring(rendered, openingTag.Position - rendered));
            builder.Append(opening);
            rendered = openingTag.Position + openingTag.Name.Length;

            foreach (var pair in tags)
            {
                builder.Append(text.Substring(rendered, pair.Item1.Position - rendered));
                rendered = pair.Item1.Position;

                if (isNested && !language.CanTagBeNestedInside(pair.Item1, openingTag))
                    continue;
                WrapInTag(pair.Item1.Position, pair.Item2.Position, pair.Item1.Name, pair.Item2.Name);
            }
            if (closingTag == null)
                return;
            var end = closingTag.Position - rendered;
            var endTag = language.ClosingHtmlTags[openingTag.Name];
            builder.Append(text.Substring(rendered, end));
            builder.Append(endTag);
            rendered = closingTag.Position + closingTag.Name.Length;
        }

        private IEnumerable<Tag> GetTags()
        {
            var hasValidContents = true;
            while (position < text.Length)
            {
                string parsedTag;
                var success = TryParseTag(position, out parsedTag);
                if (!success)
                {
                    hasValidContents = language.IsValidTagContents(text[position]);
                    position++;
                    continue;
                }
                var tag = new Tag(parsedTag, position);
                if (language.IsLineDelimiter(parsedTag))
                    yield return tag;
                position += parsedTag.Length;

                var isCodeBlock = language.IsBeginningOfCodeBlock(parsedTag);
                if (isCodeBlock || language.IsBeginningOfList(parsedTag))
                {
                    var surroundingTags = isCodeBlock ? language.CodeBlockTags : language.ListTags;
                    var entryTags = isCodeBlock ? new TagPair("", "") : language.ListEntryTag;
                    RenderMultilineTag(tag, surroundingTags, entryTags);
                    continue;
                }

                var isEscaped = language.IsEscapedTag(tag, text);
                if ((hasValidContents || !language.IsContentRestrictied(tag)) && !isEscaped)
                    yield return tag;
                if (isEscaped)
                    RenderEscapedTag(tag);
                hasValidContents = true;
            }
        }

        private bool TryParseTag(int start, out string tag)
        {
            string str = null;
            var maxLen = language.ClosingHtmlTags.Keys.Max(x => x.Length);
            for (var length = 1; length < maxLen + 1 && length + start < text.Length + 1; length++)
            {
                var subStr = text.Substring(start, length);
                if (language.HasTag(subStr))
                    str = subStr;
            }
            tag = str;
            return str != null;
        }

        private void RenderEscapedTag(Tag tag)
        {
            builder.Append(text.Substring(rendered, tag.Position - rendered - 1));
            builder.Append(tag.Name);
            rendered = tag.Position + tag.Name.Length;
        }

        private void RenderUrl(Tag opening, Tag closing)
        {
            var url = GetUrl(position);
            if (url == null)
                return;
            var title = text.Substring(opening.Position + 1, closing.Position - opening.Position - 1);
            var newRepr = language.WrapInHtmlUrlTag(title, url);
            builder.Append(newRepr);
            rendered = closing.Position + closing.Name.Length + url.Length + 2;
        }

        private string GetUrl(int from)
        {
            if (text[from].ToString() != language.UrlTags.OpeningTag)
                return null;
            while (position < text.Length)
            {
                if (text[position].ToString() == language.UrlTags.ClosingTag)
                    return text.Substring(from + 1, position - from - 1);
                position++;
            }
            return null;
        }

        private void RenderMultilineTag(Tag opening, TagPair surroundingTags, TagPair entryTags)
        {
            builder.Append(text.Substring(rendered, opening.Position - rendered));
            builder.Append(surroundingTags.OpeningTag);
            rendered = opening.Position;
            var length = text.Length;
            var previous = opening;

            while (position < length)
            {
                if (previous.Name != null)
                    RenderEntry(previous, entryTags);
                string tag;
                var success = TryParseTag(position, out tag);
                previous = new Tag(tag, position);
                position = success ? position + tag.Length : position + 1;
            }
            builder.Append(surroundingTags.ClosingTag);
        }

        private void RenderEntry(Tag starting, TagPair entryTags)
        {
            var length = text.Length;
            while (position < length)
            {
                string parsedTag;
                var success = TryParseTag(position, out parsedTag);
                position = success ? position + parsedTag.Length : position + 1;
                if ((parsedTag == null || !language.IsLineDelimiter(parsedTag)) && position < length) continue;
                builder.Append(entryTags.OpeningTag);
                builder.Append(text.Substring(rendered + starting.Name.Length, position - rendered - starting.Name.Length));
                builder.Append(entryTags.ClosingTag);
                rendered = position;
                return;
            }
        }

        private void WrapInTag(int from, int to, string openingTag, string closingTag)
        {
            var opening = language.OpeningHtmlTags[openingTag];
            var closing = language.ClosingHtmlTags[openingTag];
            var content = text.Substring(from + openingTag.Length, to - from - openingTag.Length);

            builder.Append(opening);
            builder.Append(content);
            builder.Append(closing);

            rendered = to + closingTag.Length;
        }
    }
}
