using System.Collections.Generic;

namespace Markdown
{
	class BoldTag : ITag
	{
		public string Tag { get; }
		public int Position { get; }
		public string Representation { get; }
		public bool IsOpeningTag { get; }
		private BoldTag(string tag, int position, bool isOpening, string representation)
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
			return new BoldTag(tag, positon, isOpening, representation);
		}

		public bool IsValid(string text, Stack<ITag> openingTags)
		{
			return HasValidSurroundings(text) && !IsInsideItalic(openingTags);
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
			return new BoldTag(tag, Position - 1, IsOpeningTag, Tag);
		}

		private bool HasValidSurroundings(string text)
		{
			var bias = IsOpeningTag ? Tag.Length : -1;
			if (Position + bias >= text.Length || Position + bias < 0)
				return true;
			return text[Position + bias] != ' ';
		}

		private bool IsInsideItalic(Stack<ITag> openingTags)
		{
			var isBoldTag = Tag == "__";
			var isInsideItalic = openingTags.Count > 0 && openingTags.Peek().Tag == "_";
			return isBoldTag && isInsideItalic;
		}
	}
}
