using System.Text.RegularExpressions;

namespace TelegramUsernameFinder.Helpers
{
    public class FormatUsernames
    {
        public static List<string> GetAndFormatUsername(string filename)
        {
            var usernames = File.ReadAllLines(filename).ToArray();

            Regex regexUsername   = new("[^a-zA-Z0-9_]");
            Regex stripDomainName = new("(?:www\\.)?([a-z0-9\\-]+)(?:\\.[a-z\\.]+[\\/]?).*");
            for (int i = 0; i < usernames.Length; i++)
            {
                string strippedDomainUsername = stripDomainName.Replace(usernames[i], "$1");
                string username               = regexUsername.Replace(strippedDomainUsername, "");
                if (username.Length > 0 && char.IsDigit(username.First())) continue;

                //if (username.Length < 5) username += "company";

                usernames[i] = username;
            }

            return usernames.Where(u => u.Length > 4).Distinct().ToList();
        }
    }
}
