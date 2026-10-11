
namespace CybersecurityAwarenessBot.Services
{
    public enum Sentiment
    {
        Neutral,
        Worried,
        Curious,
        Frustrated
    }

    public class SentimentDetector
    {
        public Sentiment DetectSentiment(string input)
        {
            string text = input.ToLowerInvariant();

            if (text.Contains("worried") ||
                text.Contains("scared") ||
                text.Contains("afraid") ||
                text.Contains("anxious"))
            {
                return Sentiment.Worried;
            }

            if (text.Contains("curious") ||
                text.Contains("interested") ||
                text.Contains("wondering"))
            {
                return Sentiment.Curious;
            }

            if (text.Contains("frustrated") ||
                text.Contains("annoyed") ||
                text.Contains("confused"))
            {
                return Sentiment.Frustrated;
            }

            return Sentiment.Neutral;
        }
    }
}

