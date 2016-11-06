using System;
using System.Collections.Generic;

namespace Markdown
{
	class TagTables
	{
		public static readonly Dictionary<string, string> OpeningHtlmTags = new Dictionary<string, string>
		{
			{ "_", "<em>" },
			{ "__", "<strong>" }
		};

		public static readonly Dictionary<string, string> ClosingHtlmTags = new Dictionary<string, string>
		{
			{ "_", "</em>" },
			{ "__", "</strong>" }
		};

		public static readonly Dictionary<string, Func<string, int, bool, ITag>> TagConstructors = new Dictionary
			<string, Func<string, int, bool, ITag>>
			{
				{ "_", ItalicTag.FromString },
				{ "__", BoldTag.FromString }
			};
	}
}
