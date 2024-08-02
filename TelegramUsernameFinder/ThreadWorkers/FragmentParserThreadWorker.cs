using System.Collections.Concurrent;
using TelegramUsernameFinder.Helpers;
using TelegramUsernameFinder.Repositories;

namespace TelegramUsernameFinder.ThreadWorkers
{
    public class FragmentParserThreadWorker
    {
        private readonly BlockingCollection<string> _inputQueue;
        private readonly BlockingCollection<string> _outputQueue;
        private readonly MakeRequest                _makeRequest;
        private readonly UsernameRepository         _usernameRepository;

        public FragmentParserThreadWorker(BlockingCollection<string> inputQueue, BlockingCollection<string> outputQueue, string proxy, 
                                          UsernameRepository usernameRepository)
        {
            _inputQueue         = inputQueue;
            _outputQueue        = outputQueue;
            _makeRequest        = new(proxy);
            _usernameRepository = usernameRepository;
        }

        public async Task Parse()
        {
            foreach (var username in _inputQueue.GetConsumingEnumerable())
            {
                if (_inputQueue.Count % 100 == 1)
                {
                    Console.WriteLine($"[{DateTime.Now}] " + 
                                      $"Remained source: {_inputQueue.Count}, " +
                                      $"amount to check: {_outputQueue.Count}");
                }

                //try
                //{
                    if (_usernameRepository.Get(username) != null) continue;
                //}
                //catch (Exception ex) { Console.WriteLine(ex); }

                string? answer;
                while ((answer = await _makeRequest.Work(username)) == null) Thread.Sleep(300);

                var usernameModels = AnalyzeResponse.ParseHtml(answer);
                if (usernameModels == null) continue;

                var usernameModel = usernameModels.FirstOrDefault(u => u.Id == username);
                if (usernameModel != null)
                {
                    if (usernameModel.Status.Contains("Unavailable"))
                    {
                        // Добавление username в очередь
                        _outputQueue.Add(username);

                        usernameModel.NeedTelegramCheck = true;
                    }

                    _usernameRepository.Add(usernameModel);
                    usernameModels.Remove(usernameModel);
                }

                foreach (var model in usernameModels)
                {
                    if (model.Status.Contains("Sold") && _usernameRepository.Get(model.Id) == null)
                    {
                        _usernameRepository.Add(model);
                    }
                }
            }
        }
    }
}
