using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioChapterSplitter
{
    internal class Splitter
    {
        private const string InfoArguments = "-i \"{0}\"";
        private const string SplitArguments = "-y -i \"{0}\" -ss {1} -t {2} \"{3}\"";
        private const string ChapterTitlePattern = @"title.+\:(.*)$";
        private const string ChapterTimePattern = @"start ([\d\.]+), end ([\d\.]+)";

        public static void Split(string path, string? directory = null)
        {
            string inputFilePath = GetInputFilePath(path);
            string outputDirectory = GetOutputDirectory(directory, inputFilePath);
            var inputFileName = Path.GetFileNameWithoutExtension(inputFilePath) ?? throw new Exception("Filename not valid");
            var ffmpegExecutablePath = GetFFMpegExecutablePath() ?? throw new Exception("FFMpeg not found, please install FFMPEG.");

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Console.WriteLine();
            Console.WriteLine($"Splitting '{inputFileName}'");

            var getInfoArguments = string.Format(InfoArguments, inputFilePath);

            var info = ExecuteProcess(ffmpegExecutablePath, getInfoArguments, false);

            var chapters = ParseChapters(info.output);

            Console.WriteLine();
            Console.WriteLine($"Found {chapters.Count()} chapters in '{inputFileName}'");

            foreach (var chapter in chapters)
            {
                var chapterFileName = $"{chapter.Index:000}_{GetSafeFilename(chapter.Title)}.mp3";
                var chapterFilePath = Path.Combine(outputDirectory, chapterFileName);
                var splitArguments = string.Format(SplitArguments, inputFilePath, chapter.Start, chapter.Duration, chapterFilePath);

                Console.WriteLine($"Splitting chapter {chapter.Index:000} '{chapterFileName}'");

                var splitResult = ExecuteProcess(ffmpegExecutablePath, splitArguments, false);

                if (splitResult.success)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Chapter {chapter.Index} - {chapterFileName} split successfully");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Chapter {chapter.Index} - {chapterFileName} split failed");
                    Console.ResetColor();
                }
            }
        }

        private static string GetOutputDirectory(string? directory, string inputFilePath)
        {
            string outputDirectory;
            if (!string.IsNullOrWhiteSpace(directory))
            {
                outputDirectory = directory;
            }
            else
            {
                outputDirectory = Path.GetDirectoryName(inputFilePath) ?? throw new Exception("Invalid path");
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            return outputDirectory;
        }

        private static string GetInputFilePath(string path)
        {
            string inputFilePath = Path.GetFullPath(path);

            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException("File not found", inputFilePath);
            }

            return inputFilePath;
        }

        private static string GetSafeFilename(string input, int maxLength = 100, string defaultName = "unknown")
        {
            var normalized = RemoveAccents(input);

            var name = string.Join("", normalized.Split(Path.GetInvalidFileNameChars()));

            if (string.IsNullOrWhiteSpace(name))
            {
                return defaultName;
            }

            return name.Length > maxLength ? name.Substring(0, maxLength) : name;
        }

        private static string RemoveAccents(string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string GetFFMpegExecutablePath()
        {
            var ffmpeg = "";
            var env1 = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            var env2 = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            var env = $"{env1};{env2}";
            if (env != null)
            {
                var paths = env.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var ffmpegPath = paths.FirstOrDefault(p => p.Contains("ffmpeg"));

                if (ffmpegPath != null)
                {
                    ffmpeg = Path.Combine(ffmpegPath, "ffmpeg.exe");
                }
            }

            return ffmpeg;
        }

        private static IEnumerable<Chapter> ParseChapters(string info)
        {
            string? start = null;
            string? end = null;
            string? title = null;
            int index = 0;

            var lines = info.Split(Environment.NewLine);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (start == null && end == null)
                {
                    var match = Regex.Match(line, ChapterTimePattern);
                    if (match.Success)
                    {
                        start = match.Groups[1].Value;
                        end = match.Groups[2].Value;
                    }
                }

                if (title == null)
                {
                    var match = Regex.Match(line, ChapterTitlePattern);
                    if (match.Success)
                    {
                        title = match.Groups[1].Value;
                    }
                }

                if (start != null && end != null && title != null)
                {
                    yield return new Chapter
                    {
                        Index = index++,
                        Start = double.Parse(start, CultureInfo.InvariantCulture),
                        End = double.Parse(end, CultureInfo.InvariantCulture),
                        Title = title
                    };

                    start = null;
                    end = null;
                    title = null;
                }
            }
        }

        private static (bool success, string output) ExecuteProcess(string executablePath, string arguments, bool printProcess)
        {
            StringBuilder data = new();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardInputEncoding = Encoding.UTF8,
                },
            };

            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            process.Start();

            process.StandardInput.Close();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            return (process.ExitCode == 0, data.ToString());

            void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
            {
                if (printProcess) Console.WriteLine(outLine.Data);

                data.AppendLine(outLine.Data);
            }
        }
    }
}
