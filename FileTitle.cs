namespace LinkLabelChecker
{
    public sealed class FileTitle
    {
        public FileTitle(string path, string title)
        {
            Path = path;
            Title = title;
        }

        public string Path { get; }
        public string Title { get; }
    }
}