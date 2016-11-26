using System;

namespace Markdown
{
    class Example
    {
        private static void ShowExample()
        {
            var language = new Markdown();
            var renderer = new MdRenderer(language);

            var example1 = "Basic formatting of _italics_ and __bold__ is supported.This __can be _nested_ like__ so.";
            var example2 = "Ordered list:\n1. Item 1\n2. A second item\n3.Number 3\n4. IV\n\n";
            var example3 = "Code block:\n\n   Code blocks are very useful for developers.";
            var example4 =
                "\nHeadings:\nThere are six levels of headings.\n\n#One\n##Two\n###Three\n####Four\n#####Five\n######Six\n";
            var example5 = "URLs:\nNamed link to[Repository](https://github.com/Saylala/clean-code)";
            var examples = new[] {example1, example2, example3, example4, example5};

            foreach (var example in examples)
            {
                Console.WriteLine(example);
                Console.WriteLine();
                Console.WriteLine(renderer.Render(example));
                Console.WriteLine("--------------------------------------------------------------------------------");
            }
        }

        public static void Main()
        {
            ShowExample();
        }
    }
}
