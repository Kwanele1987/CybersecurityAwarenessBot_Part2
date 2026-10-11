
namespace CybersecurityAwarenessBot.Services
{
    public class MemoryStore
    {
        public string UserName { get; set; } = string.Empty;

        public string FavouriteTopic { get; set; } = string.Empty;

        public string CurrentTopic { get; set; } = string.Empty;

        public bool HasUserName =>
            !string.IsNullOrWhiteSpace(UserName);

        public bool HasFavouriteTopic =>
            !string.IsNullOrWhiteSpace(FavouriteTopic);
    }
}

