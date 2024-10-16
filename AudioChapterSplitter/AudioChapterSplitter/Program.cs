namespace AudioChapterSplitter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("m4a splitter");
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the path of the file to split");
                Console.WriteLine("Usage: Mp3Convertor <path> [output directory]");
                Console.WriteLine(@"Example: Mp3Convertor ""C:\path\to\file.m4a"" ""C:\output\directory""");
                return;
            }

            var path = args[0];
            string? directory = args.Length > 1 ? args[1] : null;

            Console.WriteLine("Press Ctrl+C to cancel...");
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Cancelling...");
                Environment.Exit(0);
            };

            Splitter.Split(path, directory);
        }
    }
}
