using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
    public class Renderer
    {
        private int offset;
        private int currentPosition;
        private StringBuilder builder;
        private readonly IMarkupLanguage language;

        public Renderer(IMarkupLanguage language)
        {
            this.language = language;
        }

        public string Render(string text)
        {
            currentPosition = 0;
            offset = 0;
            builder = new StringBuilder(text);
            var length = text.Length;
            var previousParagraph = 0;

            while (currentPosition < length)
            {
                SkipEmtyLines(text, currentPosition);
                if (currentPosition >= length)
                    break;
                RenderNextParagraph(text);
                if (language.ParagraphsEnabled)
                    WrapInParagraph(previousParagraph - offset, currentPosition, "\n");
                previousParagraph = currentPosition;
            }

            return builder.ToString();
        }

        private void SkipEmtyLines(string text, int start)
        {
            if (text[currentPosition] != '\n')
                return;
            var currentLine = currentPosition;
            var length = text.Length;
            while (currentPosition < length && char.IsWhiteSpace(text[currentPosition]))
            {
                if (text[currentPosition] == '\n')
                    currentLine = currentPosition + 1;
                currentPosition++;
            }
            if (currentLine == start)
                return;
            builder.Remove(start + offset, currentLine - start);
            offset += start - currentLine;
        }

        private void RenderNextParagraph(string text)
        {
            var length = text.Length;

            while (currentPosition < length)
            {
                var openingTag = GetNextTag(text);
                if (openingTag == null || !language.IsTagWithValidSurroundings(text, openingTag, true))
                    continue;
                if (openingTag.Representation == "\n")
                    return;
                RenderNext(text, openingTag);
            }
        }


        private void RenderNext(string text, Tag openingTag)
        {
            var length = text.Length;
            var nestedTags = new List<Tuple<Tag, Tag>>();
            while (currentPosition < length)
            {
                var tag = GetNextTag(text);
                if (tag == null)
                    continue;
                if (tag.Representation == "\n" && openingTag.Representation[0] != '#')
                    return;
                if (tag.Representation == "\n")
                    currentPosition++;

                if (language.ArePairedTags(openingTag, tag))
                {
                    if (!language.IsTagWithValidSurroundings(text, tag, false))
                        continue;
                    if (!language.OpeningHtmlTags.ContainsKey(openingTag.Representation))
                    {
                        RenderUrl(text, openingTag, tag);
                        return;
                    }
                    WrapInTag(openingTag.Position, tag.Position, openingTag.Representation, tag.Representation);
                    RenderTags(nestedTags, tag, true);
                    return;
                }
                if (language.IsTagWithValidSurroundings(text, tag, true))
                    GetNestedTags(text, tag, openingTag, nestedTags);
            }
            RenderTags(nestedTags, null, false);
        }

        private void GetNestedTags(string text, Tag openingTag, Tag surroundingTag, List<Tuple<Tag, Tag>> output)
        {
            var length = text.Length;
            while (currentPosition < length)
            {
                var tag = GetNextTag(text);
                if (tag == null)
                    continue;

                if (tag.Representation == surroundingTag.Representation)
                {
                    if (!language.IsTagWithValidSurroundings(text, tag, false))
                        continue;
                    currentPosition -= tag.Representation.Length;
                    return;
                }

                if (tag.Representation != openingTag.Representation) continue;
                output.Add(Tuple.Create(openingTag, tag));
                return;
            }
        }

        private void RenderTags(IEnumerable<Tuple<Tag, Tag>> tags, Tag surroundingTag, bool isNested)
        {
            foreach (var pair in tags)
            {
                if (isNested && !language.CanTagBeNestedInside(pair.Item1, surroundingTag))
                    continue;
                if (!isNested)
                    WrapInTag(pair.Item1.Position, pair.Item2.Position, pair.Item1.Representation, pair.Item2.Representation);
                else
                {
                    var oldLength = surroundingTag.Representation.Length;
                    var newLength = language.OpeningHtmlTags[surroundingTag.Representation].Length;
                    var nestedBias = oldLength - newLength - 1;
                    WrapInTag(pair.Item1.Position + nestedBias, pair.Item2.Position + nestedBias, pair.Item1.Representation, pair.Item2.Representation);
                }
            }
        }

        private Tag GetNextTag(string text)
        {
            var length = text.Length;
            var hasValidContents = true;
            while (currentPosition < length)
            {
                var parsedTag = TryParseTag(text, currentPosition);
                if (parsedTag == null)
                {
                    hasValidContents = language.IsValidTagContents(text[currentPosition]);
                    currentPosition++;
                    continue;
                }
                var tag = language.GetTagFromString(parsedTag, currentPosition);
                if (parsedTag == "\n")
                    return tag;
                currentPosition += parsedTag.Length;

                if (language.IsBeginningOfCodeBlock(parsedTag))
                {
                    RenderCodeBlock(text, tag);
                    return null;
                }
                if (language.IsBeginningOfList(parsedTag))
                {
                    RenderList(text, tag);
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

        private string TryParseTag(string text, int position)
        {
            string result = null;
            var maxLen = language.ClosingHtmlTags.Keys.Max(x => x.Length);
            for (var length = 1; length < maxLen + 1 && length + position < text.Length + 1; length++)
            {
                var subStr = text.Substring(position, length);
                if (language.HasTag(subStr))
                    result = subStr;
            }
            return result;
        }

        private void UnescapeTag(Tag tag)
        {
            var pos = tag.Position - 1 + offset;
            builder.Remove(pos, 1);
            offset--;
        }

        private void RenderUrl(string text, Tag opening, Tag closing)
        {
            var url = GetUrl(text, currentPosition);
            if (url == null)
                return;
            var title = text.Substring(opening.Position + 1, closing.Position - opening.Position - 1);
            var newRepr = language.WrapInHtmlUrlTag(title, url);
            builder.Remove(opening.Position, currentPosition+1);
            builder.Insert(opening.Position, newRepr);
            offset += newRepr.Length - 1 - currentPosition + opening.Position;
        }

        private string GetUrl(string text, int from)
        {
            if (text[from] != '(')
                return null;
            while (currentPosition < text.Length)
            {
                if (text[currentPosition] == ')')
                    return text.Substring(from + 1, currentPosition - from - 1);
                currentPosition++;
            }
            return null;
        }

        private void RenderCodeBlock(string text, Tag opening)
        {
            var codeBlockTags = language.CodeBlockTags;
            builder.Insert(opening.Position + offset, codeBlockTags.Item1);
            offset += codeBlockTags.Item1.Length;
            var length = text.Length;
            var previous = opening;

            while (currentPosition < length)
            {
                if (previous.Representation != null)
                    RenderCodeBlockLine(text, previous);
                var tag = TryParseTag(text, currentPosition);
                previous = language.GetTagFromString(tag, currentPosition);
                currentPosition = tag == null ? currentPosition + 1 : currentPosition + tag.Length;
            }
            var position = Math.Min(currentPosition, text.Length);
            builder.Insert(position + offset, codeBlockTags.Item2);
            offset += codeBlockTags.Item2.Length;
        }

        private void RenderCodeBlockLine(string text, Tag starting)
        {
            var length = text.Length;
            while (currentPosition < length)
            {
                var parsedTag = TryParseTag(text, currentPosition);
                currentPosition = parsedTag == null ? currentPosition + 1 : currentPosition + parsedTag.Length;
                if ((parsedTag == null || parsedTag != "\n") && currentPosition < length) continue;
                builder.Remove(starting.Position + offset, starting.Representation.Length);
                offset -= starting.Representation.Length;
                return;
            }
        }

        private void RenderList(string text, Tag opening)
        {
            var listTags = language.ListTags;
            builder.Insert(opening.Position + offset, listTags.Item1);
            offset += listTags.Item1.Length;
            var length = text.Length;
            var previous = opening;

            while (currentPosition < length)
            {
                if (previous.Representation != null)
                    RenderListEntry(text, previous);
                var tag = TryParseTag(text, currentPosition);
                previous = language.GetTagFromString(tag, currentPosition);
                currentPosition = tag == null ? currentPosition + 1 : currentPosition + tag.Length;
            }
            var position = Math.Min(currentPosition, text.Length);
            builder.Insert(position + offset, listTags.Item2);
            offset += listTags.Item2.Length;
        }

        private void RenderListEntry(string text, Tag starting)
        {
            var length = text.Length;
            var listEntryTags = language.ListEntryTag;
            while (currentPosition < length)
            {
                var parsedTag = TryParseTag(text, currentPosition);
                currentPosition = parsedTag == null ? currentPosition + 1 : currentPosition + parsedTag.Length;
                if ((parsedTag == null || parsedTag != "\n") && currentPosition < length) continue;
                builder.Insert(currentPosition + offset, listEntryTags.Item2);
                builder.Remove(starting.Position + offset, starting.Representation.Length);
                builder.Insert(starting.Position + offset, listEntryTags.Item1);
                offset += listEntryTags.Item1.Length + listEntryTags.Item2.Length - starting.Representation.Length;
                return;
            }
        }

        private void WrapInParagraph(int from, int to, string tag)
        {
            var opening = language.OpeningHtmlTags[tag];
            var closing = language.ClosingHtmlTags[tag];

            builder.Insert(to + offset, closing);
            builder.Insert(Math.Max(0, from + offset), opening);

            offset += opening.Length + closing.Length;
        }

        private void WrapInTag(int from, int to, string openingTag, string closingTag)
        {
            var opening = language.OpeningHtmlTags[openingTag];
            var closing = language.ClosingHtmlTags[openingTag];

            builder.Remove(to + offset, closingTag.Length);
            builder.Insert(to + offset, closing);
            builder.Remove(from + offset, openingTag.Length);
            builder.Insert(from + offset, opening);

            offset += opening.Length + closing.Length - openingTag.Length - closingTag.Length;
        }
    }
}
