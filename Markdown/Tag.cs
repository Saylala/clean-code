namespace Markdown
{
    public class Tag
    {
        public string Name { get; }
        public int Position { get; }
        public Tag(string tag, int position)
        {
            Name = tag;
            Position = position;
        }
    }
}
