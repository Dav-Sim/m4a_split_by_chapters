namespace AudioChapterSplitter
{
    internal class Chapter
    {
        public string Title { get; set; } = null!;
        public int Index { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
        public double Duration => End - Start;
    }
}
