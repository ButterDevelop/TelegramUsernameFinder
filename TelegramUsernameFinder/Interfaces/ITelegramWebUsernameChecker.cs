using OpenQA.Selenium;

namespace TelegramUsernameFinder.Interfaces
{
    public interface ITelegramWebUsernameChecker
    {
        IWebDriver InitializeWebDriver();
        bool IsUsernameAvailable(string username);
    }
}
