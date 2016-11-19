namespace Markdown
{
    public class Tag
    {
        public string Representation { get; }
        public int Position { get; }
        public Tag(string tag, int position)
        {
            Representation = tag;
            Position = position;
        }
    }
}
