using TelegramUsernameFinder.Helpers;
using TelegramUsernameFinder.ThreadWorkers;

namespace TelegramUsernameFinder
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            const string baseInputFilename = "words_alpha.txt";//"companies_sorted.csv.001";

            string inputFilename  = Path.Combine("input", baseInputFilename),
                   outputFilename = Path.Combine("output", "output_" + baseInputFilename);

            if (!File.Exists(outputFilename)) File.WriteAllText(outputFilename, "");

            string[] profileNames = ["Profile 6", "Profile 7", "Profile 8", "Profile 9"];
            int parserCount = 10, checkerCount = profileNames.Length;

            MainThreadWorker mainThreadWorker = new(parserCount, checkerCount, outputFilename);
            mainThreadWorker.AddSource(FormatUsernames.GetAndFormatUsername(inputFilename));

            string profileDataPath = Environment.CurrentDirectory;
            List<(string, string)> checkers =
            [
                new(Path.Combine(profileDataPath, profileNames[0].Replace("Profile ", "")), profileNames[0]),
                new(Path.Combine(profileDataPath, profileNames[1].Replace("Profile ", "")), profileNames[1]),
                new(Path.Combine(profileDataPath, profileNames[2].Replace("Profile ", "")), profileNames[2]),
                new(Path.Combine(profileDataPath, profileNames[3].Replace("Profile ", "")), profileNames[3]),
            ];
            List<string> proxies = Enumerable.Repeat("", parserCount).ToList();

            await Task.Run(() => mainThreadWorker.StartAsync(proxies, checkers));
        }
    }
}