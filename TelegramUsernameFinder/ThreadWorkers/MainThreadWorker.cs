using System.Collections.Concurrent;
using TelegramUsernameFinder.Repositories;

namespace TelegramUsernameFinder.ThreadWorkers
{
    public class MainThreadWorker
    {
        private readonly BlockingCollection<string> _inputQueue;
        private readonly BlockingCollection<string> _outputQueue;
        private readonly BlockingCollection<string> _resultQueue;
        private readonly int    _parserCount;
        private readonly int    _checkerCount;
        private readonly string _outputFilename;

        public MainThreadWorker(int parserCount, int checkerCount, string outputFilename)
        {
            _inputQueue  = [];
            _outputQueue = [];
            _resultQueue = [];

            _parserCount    = parserCount;
            _checkerCount   = checkerCount;
            _outputFilename = outputFilename;
        }

        public void AddSource(List<string> sourceUsernames)
        {
            // Заполняем очередь исходными данными
            foreach (var username in sourceUsernames)
            {
                _inputQueue.Add(username);
            }
            _inputQueue.CompleteAdding();
        }

        public void SaveToFile()
        {
            foreach (var availableUsername in _resultQueue.GetConsumingEnumerable())
            {
                Console.WriteLine($"[{DateTime.Now}] Good username: {availableUsername}");
                File.AppendAllText(_outputFilename, availableUsername + Environment.NewLine);
            }
        }

        public async Task StartAsync(List<string> proxies, List<(string, string)> checkers)
        {
            if (_parserCount  > proxies.Count)  throw new Exception("Not enough proxies.");
            if (_checkerCount > checkers.Count) throw new Exception("Not enough checkers.");

            UsernameRepository usernameRepository = new(Config.MONGO_CONNECTION_STRING, Config.MONGO_DATABASE_NAME, Config.MONGO_USERNAME_PATH);

            // Запуск потоков парсинга
            var parserTasks = new List<Task>();
            for (int i = 0; i < _parserCount; i++)
            {
                var worker = new FragmentParserThreadWorker(_inputQueue, _outputQueue, proxies[i], usernameRepository);
                parserTasks.Add(Task.Run(worker.Parse));
                Thread.Sleep(100);
            }

            // Запуск потоков проверки
            var checkerTasks = new List<Task>();
            for (int i = 0; i < _checkerCount; i++)
            {
                var checker = new SeleniumCheckerThreadWorker(_outputQueue, _resultQueue, checkers[i], usernameRepository);
                checkerTasks.Add(Task.Run(checker.Check));
                Thread.Sleep(1000);
            }

            await Task.Run(SaveToFile);

            // Дожидаемся завершения всех задач
            await Task.WhenAll(parserTasks);
            _outputQueue.CompleteAdding();
            await Task.WhenAll(checkerTasks);
            _resultQueue.CompleteAdding();
        }
    }
}
