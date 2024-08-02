namespace TelegramUsernameFinder
{
    public class Utils
    {
        private const string _chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        private readonly Random _random;

        public Utils()
        {
            _random = new();
        }

        public string RandomString(int length)
        {
            return new string(Enumerable.Repeat(_chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public string RandomString(int minLength, int maxLength)
        {
            int length = _random.Next(minLength, maxLength + 1);
            return RandomString(length);
        }

        public int RandomInt(int min, int max)
        {
            return _random.Next(min, max + 1);
        }
    }
}
