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
            builder.Append(language.BaseTags.Item1);
            while (position < text.Length)
            {
                SkipEmtyLines(position);
                if (position >= text.Length)
                    break;
                builder.Append(language.ParagraphTags.Item1);
                RenderNextParagraph();
                builder.Append(language.ParagraphTags.Item2);
            }
            builder.Append(language.BaseTags.Item2);
            return builder.ToString();
        }

        private void SkipEmtyLines(int start)
        {
            if (text[position].ToString() != language.StringDelimiter)
                return;
            var currentLine = start;
            var length = text.Length;
            while (position < length && char.IsWhiteSpace(text[position]))
            {
                if (text[position].ToString() == language.StringDelimiter)
                    currentLine = position + 1;
                position++;
            }
            rendered = currentLine;
        }

        private void RenderNextParagraph()
        {
            while (position < text.Length)
            {
                var openingTag = GetNextTag();
                var end = openingTag == null ? text.Length : openingTag.Position;
                builder.Append(text.Substring(rendered, end - rendered));
                rendered = end;
                if (openingTag == null || !language.IsTagWithValidSurroundings(text, openingTag, true))
                    continue;
                if (openingTag.Name == language.StringDelimiter)
                    return;
                RenderNextTagPair(openingTag);
            }
        }


        private void RenderNextTagPair(Tag openingTag)
        {
            var nestedTags = new List<Tuple<Tag, Tag>>();
            while (position < text.Length)
            {
                var tag = GetNextTag();
                if (tag == null)
                    continue;
                if (tag.Name == language.StringDelimiter && ! language.IsHeaderTag(openingTag.Name))
                    return;
                if (tag.Name == language.StringDelimiter)
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
                    GetNestedTags(tag, openingTag, nestedTags);
            }
            RenderTags(nestedTags, openingTag, null, false);
        }

        private void GetNestedTags(Tag openingTag, Tag surroundingTag, List<Tuple<Tag, Tag>> output)
        {
            while (position < text.Length)
            {
                var tag = GetNextTag();
                if (tag == null)
                    continue;
                if (tag.Name == surroundingTag.Name)
                {
                    if (!language.IsTagWithValidSurroundings(text, tag, false))
                        continue;
                    position -= tag.Name.Length;
                    return;
                }
                if (tag.Name != openingTag.Name) continue;
                output.Add(Tuple.Create(openingTag, tag));
                return;
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

            var end = closingTag == null ? text.Length - rendered : closingTag.Position - rendered;
            var endTag = closingTag == null ? string.Empty : language.ClosingHtmlTags[openingTag.Name];
            builder.Append(text.Substring(rendered, end));
            builder.Append(endTag);
            rendered = closingTag == null ? text.Length - rendered : closingTag.Position + closingTag.Name.Length;
        }

        private Tag GetNextTag()
        {
            var hasValidContents = true;
            while (position < text.Length)
            {
                var parsedTag = TryParseTag(position);
                if (parsedTag == null)
                {
                    hasValidContents = language.IsValidTagContents(text[position]);
                    position++;
                    continue;
                }
                var tag = new Tag(parsedTag, position);
                if (parsedTag == language.StringDelimiter)
                    return tag;
                position += parsedTag.Length;

                var isCodeBlock = language.IsBeginningOfCodeBlock(parsedTag);
                if (isCodeBlock || language.IsBeginningOfList(parsedTag))
                {
                    var surroundingTags = isCodeBlock ? language.CodeBlockTags : language.ListTags;
                    var entryTags = isCodeBlock ? Tuple.Create("", "") : language.ListEntryTag;
                    RenderMultilineTag(tag, surroundingTags, entryTags);
                    return null;
                }

                var isEscaped = language.IsEscapedTag(tag, text);
                if ((hasValidContents || !language.IsContentRestrictied(tag)) && !isEscaped)
                    return tag;
                if (isEscaped)
                    UnescapeTag(tag);
                hasValidContents = true;
            }
            return null;
        }

        private string TryParseTag(int start)
        {
            string result = null;
            var maxLen = language.ClosingHtmlTags.Keys.Max(x => x.Length);
            for (var length = 1; length < maxLen + 1 && length + start < text.Length + 1; length++)
            {
                var subStr = text.Substring(start, length);
                if (language.HasTag(subStr))
                    result = subStr;
            }
            return result;
        }

        private void UnescapeTag(Tag tag)
        {
            var tempBulder = new StringBuilder(text);
            tempBulder.Remove(tag.Position - 1, 1);
            text = tempBulder.ToString();
            position--;
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
            if (text[from].ToString() != language.UrlTags.Item1)
                return null;
            while (position < text.Length)
            {
                if (text[position].ToString() == language.UrlTags.Item2)
                    return text.Substring(from + 1, position - from - 1);
                position++;
            }
            return null;
        }

        private void RenderMultilineTag(Tag opening, Tuple<string, string> surroundingTags, Tuple<string, string> entryTags)
        {
            builder.Append(text.Substring(rendered, opening.Position - rendered));
            builder.Append(surroundingTags.Item1);
            rendered = opening.Position;
            var length = text.Length;
            var previous = opening;

            while (position < length)
            {
                if (previous.Name != null)
                    RenderEntry(previous, entryTags);
                var tag = TryParseTag(position);
                previous = new Tag(tag, position);
                position = tag == null ? position + 1 : position + tag.Length;
            }
            builder.Append(surroundingTags.Item2);
        }

        private void RenderEntry(Tag starting, Tuple<string, string> entryTags)
        {
            var length = text.Length;
            while (position < length)
            {
                var parsedTag = TryParseTag(position);
                position = parsedTag == null ? position + 1 : position + parsedTag.Length;
                if ((parsedTag == null || parsedTag != language.StringDelimiter) && position < length) continue;
                builder.Append(entryTags.Item1);
                builder.Append(text.Substring(rendered + starting.Name.Length, position - rendered - starting.Name.Length));
                builder.Append(entryTags.Item2);
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
