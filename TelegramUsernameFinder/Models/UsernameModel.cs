namespace TelegramUsernameFinder.Models
{
    public class UsernameModel
    {
        public string Id                { get; set; } = string.Empty;
        public int    Value             { get; set; }
        public string Status            { get; set; } = string.Empty;
        public bool   IsAvailable       { get; set; }
        public bool   NeedTelegramCheck { get; set; }
    }
}
