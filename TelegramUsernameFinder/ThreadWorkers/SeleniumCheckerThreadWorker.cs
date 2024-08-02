using System.Collections.Concurrent;
using TelegramUsernameFinder.Repositories;
using TelegramUsernameFinder.UsernameCheckers;

namespace TelegramUsernameFinder.ThreadWorkers
{
    public class SeleniumCheckerThreadWorker
    {
        private readonly BlockingCollection<string> _inputQueue;
        private readonly BlockingCollection<string> _outputQueue;
        private readonly (string, string)           _checkerParameters;
        private readonly UsernameRepository         _usernameRepository;

        public SeleniumCheckerThreadWorker(BlockingCollection<string> inputQueue,
                                           BlockingCollection<string> outputQueue, 
                                           (string, string) checkerParameters,
                                           UsernameRepository usernameRepository)
        {
            _inputQueue         = inputQueue;
            _outputQueue        = outputQueue;
            _checkerParameters  = checkerParameters;
            _usernameRepository = usernameRepository;
        }

        public void Check()
        {
            TelegramWebUsernameBotUsernameChecker checker = new(_checkerParameters.Item1, _checkerParameters.Item2);

            foreach (var username in _inputQueue.GetConsumingEnumerable())
            {
                // Проверка username через Selenium
                bool isAvailable = checker.IsUsernameAvailable(username);
                if (isAvailable)
                {
                    _outputQueue.Add(username);

                    var model = _usernameRepository.Get(username);
                    if (model != null)
                    {
                        model.IsAvailable       = true;
                        model.NeedTelegramCheck = false;
                        _usernameRepository.Update(model);
                    }
                }
            }
        }
    }
}
