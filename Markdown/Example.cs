using System.IO;

namespace Markdown
{
    class Example
    {
	    public static void RenderExample(string exampleFileName, string ouputFileName)
	    {
		    var language = new Markdown();
			var renderer = new MdRenderer(language);

		    var text = File.ReadAllText(exampleFileName);
		    var result = renderer.Render(text);

			File.WriteAllText($"{ouputFileName}.html", result);
	    }

        public static void Main()
        {
            RenderExample("example.txt", "example");
        }
    }
}
