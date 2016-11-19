using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown
{
    public class Renderer
    {
        private int bias;
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
            bias = 0;
            builder = new StringBuilder(text);
            var length = text.Length;

            while (currentPosition < length)
            {
                var openingTag = GetNextTag(text);
                if (openingTag == null || !language.IsTagWithValidSurroundings(text, openingTag, true))
                    continue;
                RenderNext(text, openingTag);
            }

            return builder.ToString();
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

                if (language.ArePairedTags(openingTag, tag))
                {
                    if (!language.IsTagWithValidSurroundings(text, tag, false))
                        continue;
                    WrapInTag(openingTag.Position, tag.Position, tag);
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
                    WrapInTag(pair.Item1.Position, pair.Item2.Position, pair.Item1);
                else
                {
                    var oldLength = surroundingTag.Representation.Length;
                    var newLength = language.OpeningHtmlTags[surroundingTag.Representation].Length;
                    var nestedBias = oldLength - newLength - 1;
                    WrapInTag(pair.Item1.Position + nestedBias, pair.Item2.Position + nestedBias, pair.Item1);
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
                currentPosition += parsedTag.Length;

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
            for (var length = 1; position + length < text.Length + 1; length++)
            {
                var subStr = text.Substring(position, length);
                if (!language.HasTag(subStr))
                    break;
                result = subStr;
            }
            return result;
        }

        private void UnescapeTag(Tag tag)
        {
            var pos = tag.Position - 1 + bias;
            builder.Remove(pos, 1);
            bias--;
        }

        private void WrapInTag(int from, int to, Tag tag)
        {
            var opening = language.OpeningHtmlTags[tag.Representation];
            var closing = language.ClosingHtmlTags[tag.Representation];

            builder.Remove(to + bias, tag.Representation.Length);
            builder.Insert(to + bias, closing);
            builder.Remove(from + bias, tag.Representation.Length);
            builder.Insert(from + bias, opening);

            bias += opening.Length + closing.Length - 2 * tag.Representation.Length;
        }
    }
}
