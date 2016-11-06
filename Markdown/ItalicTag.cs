using System.Collections.Generic;

namespace Markdown
{
	class ItalicTag : ITag
	{
		public string Tag { get; }
		public int Position { get; }
		public bool IsOpeningTag { get; }
		public string Representation { get; }

		private ItalicTag(string tag, int position, bool isOpening, string representation)
		{
			Tag = tag;
			Position = position;
			IsOpeningTag = isOpening;
			Representation = representation;
		}

		public static ITag FromString(string tag, int positon, bool isOpening)
		{
			var tagsTable = isOpening ? TagTables.OpeningHtlmTags : TagTables.ClosingHtlmTags;
			var representation = tagsTable[tag];
			return new ItalicTag(tag, positon, isOpening, representation);
		}

		public bool IsValid(string text, Stack<ITag> openingTags)
		{
			return HasValidSurroundings(text);
		}

		public bool IsEscaped(string text)
		{
			if (Position == 0)
				return false;
			return text[Position - 1] == '\\';
		}

		public ITag GetUnescapedTag()
		{
			var tag = "\\" + Tag;
			return new ItalicTag(tag, Position - 1, IsOpeningTag, Tag);
		}

		private bool HasValidSurroundings(string text)
		{
			var bias = IsOpeningTag ? Tag.Length : -1;
			if (Position + bias >= text.Length || Position + bias < 0)
				return true;
			return text[Position + bias] != ' ';
		}
	}
}
