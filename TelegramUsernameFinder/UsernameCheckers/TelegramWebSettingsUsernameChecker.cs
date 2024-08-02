using TelegramUsernameFinder.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Keys = OpenQA.Selenium.Keys;

namespace TelegramUsernameFinder.UsernameCheckers
{
    public class TelegramWebSettingsUsernameChecker : ITelegramWebUsernameChecker
    {
        private readonly string     _profilePath;
        private readonly string     _profileName;
        private readonly string     _userAgent;
        private readonly IWebDriver _driver;

        public TelegramWebSettingsUsernameChecker(string profilePath, string profileName, string userAgent)
        {
            _profilePath = profilePath;
            _profileName = profileName;
            _userAgent   = userAgent;
            _driver      = InitializeWebDriver();
        }

        public IWebDriver InitializeWebDriver()
        {
            ChromeOptions options = new();
            options.AddArgument($"--user-data-dir={_profilePath}");
            options.AddArgument($"--profile-directory={_profileName}");
            options.AddArgument("--disable-gpu"); // Отключение GPU для экономии ресурсов
            options.AddArgument("--no-sandbox"); // Отключение песочницы
            options.AddArgument("--disable-dev-shm-usage"); // Отключение использования /dev/shm
            options.AddArgument("--window-size=300,300"); // Уменьшение размера окна браузера
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-extensions");
            //options.AddArgument($"--user-agent={_userAgent}");
            //options.AddArgument($"user-agent={_userAgent}");

            //options.AddArgument("--headless"); // Опция для экономии ресурсов

            var browser = new ChromeDriver(options);
            Thread.Sleep(5000);

            // Переход к настройкам профиля для проверки username
            browser.Navigate().GoToUrl("https://web.telegram.org/a/");
            Thread.Sleep(5000); // Ожидание загрузки страницы настроек

            IWebElement button = browser.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div/div[1]/div/div[1]/button"));
            button.Click();
            Thread.Sleep(1000);

            IWebElement settings = browser.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div/div[1]/div/div[1]/div/div[2]/div[5]"));
            settings.Click();
            Thread.Sleep(1000);

            IWebElement edit = browser.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div[2]/div/div/div[1]/div/button"));
            edit.Click();
            Thread.Sleep(1000);

            return browser;
        }

        public bool IsUsernameAvailable(string usernameToCheck)
        {
            try
            {
                // Ввод username для проверки
                IWebElement usernameInput = _driver.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div[2]/div/div[2]/div[2]/div/div[2]/div/input"));

                usernameInput.Click();
                usernameInput.SendKeys(Keys.LeftControl + "A");
                usernameInput.SendKeys(Keys.Backspace);
                foreach (var c in usernameToCheck)
                {
                    usernameInput.SendKeys(c.ToString());
                }

                int counter = 0;
                while (counter++ < 10)
                {
                    Thread.Sleep(500); // Ожидание проверки доступности

                    // Проверка, подсвечен ли username красным
                    IWebElement usernameStatus = _driver.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div[2]/div/div[2]/div[2]/div/div[2]/div/label"));
                    string statusText = usernameStatus.Text;

                    if (statusText.Contains("Checking")) continue;

                    if (statusText.Contains("is available"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }
    }
}
