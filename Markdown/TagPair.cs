namespace Markdown
{
    public class TagPair
    {
        public string OpeningTag { get; private set; }
        public string ClosingTag { get; private set; }
        public TagPair(string opening, string closing)
        {
            OpeningTag = opening;
            ClosingTag = closing;
        }
    }
}
