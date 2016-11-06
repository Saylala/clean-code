using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
	class Md
	{
		public string Render(string markdownText)
		{
			var tags = GetTags(markdownText);
			return ReplaceTags(markdownText, tags);
		}

		private IEnumerable<ITag> GetTags(string text)
		{
			var position = 0;
			var openingTags = new Stack<ITag>();
			var tags = new List<ITag>();          //new SortedList<Tuple<string, int>, int>();???
			var tagConstructors = TagTables.TagConstructors;

			while (position < text.Length)
			{
				var parsedTag = TryParseTag(text, position);
				if (parsedTag == null)
				{
					position++;
					continue;
				}

				var isOpeningTag = openingTags.Count == 0 || openingTags.Peek().Tag != parsedTag;
				var tag = tagConstructors[parsedTag].Invoke(parsedTag, position, isOpeningTag);
				position += parsedTag.Length;

				ManageTags(text, tag, openingTags, tags);
			}
			return tags.OrderByDescending(x => x.Position);
		}

		private string TryParseTag(string text, int position)
		{
			string result = null;
			foreach (var tag in TagTables.OpeningHtlmTags.Keys.OrderBy(x => x.Length))
			{
				if (position + tag.Length > text.Length)
					continue;
				var subStr = text.Substring(position, tag.Length);
				if (subStr == tag)
					result = tag;
			}
			return result;
		}

		private void ManageTags(string text, ITag tag, Stack<ITag> openingTags, List<ITag> tags)
		{
			if (tag.IsEscaped(text))
			{
				tags.Add(tag.GetUnescapedTag());
				return;
			}
			if (!tag.IsValid(text, openingTags))
				return;
			if (tag.IsOpeningTag)
				openingTags.Push(tag);
			else
			{
				var openingTag = openingTags.Pop();
				if (!IsValidTagPair(text, openingTag, tag))
					return;
				tags.Add(tag);
				tags.Add(openingTag);
			}
		}

		private bool IsValidTagPair(string text, ITag openingTag, ITag closingTag)
		{
			var textInTags = text.Substring(openingTag.Position, closingTag.Position - openingTag.Position);
			return !textInTags.Any(char.IsDigit);
		}

		private string ReplaceTags(string text, IEnumerable<ITag> tags)
		{
			var builder = new StringBuilder(text);
			foreach (var tag in tags)
				ReplaceTag(builder, tag);
			return builder.ToString();
		}

		private void ReplaceTag(StringBuilder builder, ITag tag)
		{
			builder.Remove(tag.Position, tag.Tag.Length);
			builder.Insert(tag.Position, tag.Representation);
		}
	}
}
