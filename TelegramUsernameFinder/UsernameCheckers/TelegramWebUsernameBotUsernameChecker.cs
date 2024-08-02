using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumUndetectedChromeDriver;
using System.Text.RegularExpressions;
using TelegramUsernameFinder.Interfaces;
using Keys = OpenQA.Selenium.Keys;

namespace TelegramUsernameFinder.UsernameCheckers
{
    public class TelegramWebUsernameBotUsernameChecker : ITelegramWebUsernameChecker
    {
        private readonly string     _profilePath;
        private readonly string     _profileName;
        private readonly Utils      _utils;
        private readonly IWebDriver _driver;

        private bool _doNotStartNextTime;

        public TelegramWebUsernameBotUsernameChecker(string profilePath, string profileName)
        {
            _profilePath = profilePath;
            _profileName = profileName;
            _utils       = new();
            _driver      = InitializeWebDriver();

            _doNotStartNextTime = false;
        }

        public IWebDriver InitializeWebDriver()
        {
            ChromeOptions options = new();
            options.AddArgument($"--profile-directory={_profileName}");
            options.AddArgument("--disable-gpu"); // Отключение GPU для экономии ресурсов
            options.AddArgument("--disable-dev-shm-usage"); // Отключение использования /dev/shm
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-extensions");

            UndetectedChromeDriver browser =
                UndetectedChromeDriver.Create
            (
                options:               options,
                driverExecutablePath:  Path.GetFullPath("chromedriver.exe"),
                headless:              true,
                userDataDir:           _profilePath,
                noSandbox:             true
            );

            // Переход к настройкам профиля для проверки username
            browser.GoToUrl("https://web.telegram.org/a/#835818685");
            Thread.Sleep(5000); // Ожидание загрузки страницы настроек

            browser.Manage().Window.Size = new Size(300, 300);

            Thread.Sleep(1000);

            browser.GetScreenshot().SaveAsFile("screenshots\\" + _profileName + ".png");

            browser.Manage().Window.Minimize();

            return browser;
        }

        private IWebElement? GetLastMessage()
        {
            try
            {
                return _driver.FindElements(By.ClassName("Message")).OrderBy(m => m.GetAttribute("id")).Last();
            }
            catch
            {
                return null;
            }
        }
        private int GetIdFromMessage(IWebElement? webElement)
        {
            if (webElement == null) return 0;

            Regex regex = new("message-(\\d*)-?");
            string messageIdText = regex.Match(webElement.GetAttribute("id")).Groups[1].Value;

            if (!int.TryParse(messageIdText, out int result)) return 0;

            return result;
        }
        private int GetLastMessageId()
        {
            return GetIdFromMessage(GetLastMessage());
        }

        private int WaitForNewMessages(int lastMessageIdBefore)
        {
            int attempts = 0, lastMessageIdResult = 0;
            while (attempts++ < 40 && (lastMessageIdResult = GetLastMessageId()) != lastMessageIdBefore + 2) Thread.Sleep(25);
            return lastMessageIdResult;
        }

        public bool IsUsernameAvailable(string usernameToCheck)
        {
            try
            {
                // Ввод username для проверки
                IWebElement usernameInput = _driver.FindElement(By.Id("editable-message-text"));

                // Scroll down to page
                var elements = _driver.FindElements(By.XPath("/html/body/div[2]/div/div[2]/div[4]/div[3]/div[3]/button"));
                if (elements.Count > 0) elements[0].Click();

                int lastMessageAfterStart = 0;
                if (_doNotStartNextTime)
                {
                    _doNotStartNextTime   = false;
                    lastMessageAfterStart = GetLastMessageId();
                }
                else
                {
                    int lastMessageBeforeStart = GetLastMessageId();

                    string command = "/" + _utils.RandomString(3, 10);

                    usernameInput.Click();
                    usernameInput.SendKeys(Keys.LeftControl + "A");
                    usernameInput.SendKeys(Keys.Backspace);
                    foreach (var c in command) usernameInput.SendKeys(c.ToString());
                    usernameInput.SendKeys(Keys.Enter);

                    lastMessageAfterStart = WaitForNewMessages(lastMessageBeforeStart);
                }

                usernameInput.Click();
                usernameInput.SendKeys(Keys.LeftControl + "A");
                usernameInput.SendKeys(Keys.Backspace);
                foreach (var c in usernameToCheck) usernameInput.SendKeys(c.ToString());
                usernameInput.SendKeys(Keys.Enter);

                WaitForNewMessages(lastMessageAfterStart);

                IWebElement? lastMessage;
                int attemptsLastMessage = 0;
                while ((lastMessage = GetLastMessage()) == null && attemptsLastMessage++ < 20)
                {
                    lastMessage = GetLastMessage();
                    Thread.Sleep(25);
                }
                if (lastMessage == null) return true;

                string lastMessageText = lastMessage.Text;

                if (lastMessageText.Contains("Do you own the"))
                {
                    try
                    {
                        const string answerWhenAskAboutOtherSocialMedias = "No";

                        usernameInput.Click();
                        usernameInput.SendKeys(Keys.LeftControl + "A");
                        usernameInput.SendKeys(Keys.Backspace);
                        foreach (var c in answerWhenAskAboutOtherSocialMedias) usernameInput.SendKeys(c.ToString());
                        usernameInput.SendKeys(Keys.Enter);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Yes/No btn error: {ex}");
                    }
                }
                else
                if (lastMessageText.Contains("Sorry, this username can't be used on Telegram at the moment"))
                {
                    _doNotStartNextTime = true;
                }

                return lastMessageText.Contains("This username is currently not assigned");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex}");
                return false;
            }
        }
    }
}
