using System.Collections.Generic;

namespace Markdown
{
	interface ITag
	{
		string Tag { get; }
		int Position { get; }
		bool IsOpeningTag { get; }
		string Representation { get; }
		bool IsValid(string text, Stack<ITag> openingTags);
		bool IsEscaped(string text);
		ITag GetUnescapedTag();
	}
}
