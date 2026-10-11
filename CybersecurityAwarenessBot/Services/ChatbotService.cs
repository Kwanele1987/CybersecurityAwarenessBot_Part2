namespace CybersecurityAwarenessBot.Services;

public class ChatbotService
{
    private readonly Random _random = new();

    private readonly KeywordResponder _keywordResponder = new();
    private readonly MemoryStore _memory = new();
    private readonly SentimentDetector _sentimentDetector = new();

    public string UserName { get; set; } = "Kwanele";
    public string FavouriteTopic { get; set; } = "Password Safety";

    private readonly Dictionary<string, List<string>> _responses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["password"] = new()
        {
            "Use a long, unique password or passphrase for each important account. A password manager can help you manage unique passwords.",
            "Avoid using names, birthdays, school details, or other easy-to-guess personal information in passwords.",
            "Where available, enable multi-factor authentication as an extra layer of account protection."
        },
        ["phishing"] = new()
        {
            "Be cautious with unexpected messages asking for passwords, payment details, or urgent action.",
            "Before clicking a link, check the sender and destination carefully. When possible, visit the organisation through its official website or app.",
            "Scammers can imitate trusted organisations. Verify unusual requests using a trusted contact method."
        },
        ["privacy"] = new()
        {
            "Review privacy settings on your accounts and apps so you understand what information is being shared.",
            "Only provide personal information when it is necessary and you understand who is receiving it.",
            "Check app permissions regularly and remove permissions that are no longer needed."
        },
        ["browsing"] = new()
        {
            "Keep your operating system, browser, and security software updated.",
            "Look for secure, expected website addresses and be cautious of unexpected downloads.",
            "Do not ignore browser security warnings without first understanding why they appeared."
        },
        ["scam"] = new()
        {
            "If an offer or message creates unusual urgency, pause and verify it independently before taking action.",
            "Never share passwords or verification codes just because someone asks for them in a message or call.",
            "If something seems too good to be true, check the claim through a trusted source."
        }
    };

    public string DetectSentiment(string input)
    {
        if (ContainsAny(input, "worried", "scared", "concerned", "anxious", "unsure"))
            return "worried";
        if (ContainsAny(input, "curious", "interested", "wondering", "learn"))
            return "curious";
        if (ContainsAny(input, "frustrated", "confused", "annoyed", "stuck"))
            return "frustrated";
        return "neutral";
    }

    public string GetResponse(string input, string sentiment, out string intent)
    {
        string lower = input.ToLowerInvariant();

        // Remember the current user details.
        _memory.UserName = UserName;
        _memory.FavouriteTopic = FavouriteTopic;

        // Use the sentiment detector for consistent classification.
        Sentiment detectedSentiment =
            _sentimentDetector.DetectSentiment(input);

        if (detectedSentiment != Sentiment.Neutral)
        {
            sentiment = detectedSentiment.ToString().ToLowerInvariant();
        }

        // Recall the user's name from session memory.
        if (ContainsAny(lower,
            "what is my name",
            "what's my name",
            "do you remember my name"))
        {
            intent = "memory";
            return _memory.HasUserName
                ? $"Your name is {_memory.UserName}. I remember it from our conversation."
                : "You have not told me your name yet.";
        }

        // Recall the favourite topic from session memory.
        if (ContainsAny(lower,
            "what is my favourite topic",
            "what is my favorite topic",
            "what topic do i like"))
        {
            intent = "memory";
            return _memory.HasFavouriteTopic
                ? $"Your favourite cybersecurity topic is {_memory.FavouriteTopic}."
                : "You have not told me your favourite topic yet.";
        }

        // Continue the last cybersecurity topic.
        if (ContainsAny(lower,
            "tell me more",
            "another tip",
            "what else",
            "explain more"))
        {
            string previousTopic = _memory.CurrentTopic;

            if (!string.IsNullOrWhiteSpace(previousTopic))
            {
                intent = previousTopic;
                return ApplySentiment(
                    _keywordResponder.GetRandomResponse(previousTopic),
                    sentiment);
            }
        }

        if ((lower.Contains("worried") || lower.Contains("concerned") || lower.Contains("anxious"))
    && lower.Contains("security"))
        {
            intent = "sentiment";
            return $"I understand, {UserName}. Online security can feel confusing, but taking small steps like using strong passwords, checking links carefully, and enabling multi-factor authentication can make a big difference.";
        }
        if (ContainsAny(lower, "what is my favourite topic", "what is my favorite topic", "what topic do i like"))
        {
            intent = "memory";
            return $"Your favourite cybersecurity topic is {FavouriteTopic}.";
        }
        if (ContainsAny(lower, "how are you", "how are u"))
        {
            intent = "general";
            return $"I'm doing well, {UserName}! I'm ready to help you learn about cybersecurity.";
        }

        if (ContainsAny(lower, "what is your purpose", "what is your purpose", "what do you do"))
        {
            intent = "general";
            return "My purpose is to provide practical cybersecurity awareness guidance through conversation, tasks, reminders, and a quiz.";
        }

        if (ContainsAny(lower, "what can i ask", "what can you help", "help me"))
        {
            intent = "general";
            return "You can ask me about password safety, phishing, scams, privacy, safe browsing, reminders, tasks, the quiz, and your activity log.";
        }

        if (ContainsAny(lower, "start quiz", "take quiz", "cyber quiz", "quiz me", "mini game"))
        {
            intent = "quiz";
            return "Opening the cybersecurity quiz now.";
        }

        if (ContainsAny(lower, "show activity", "activity log", "what have you done"))
        {
            intent = "log";
            return "Opening the activity log so you can review recent actions.";
        }

        if (ContainsAny(lower, "add task", "create task", "set a task", "remind me", "set reminder", "add a reminder"))
        {
            intent = "add_task";
            return "I recognised this as a task or reminder request.";
        }

        if (ContainsAny(lower, "task list", "show tasks", "my tasks", "view tasks"))
        {
            intent = "tasks";
            return "Opening your task list.";
        }

        if (ContainsAny(lower, "another tip", "more", "explain more", "tell me more"))
        {
            intent = "follow_up";
            return FollowUpResponse();
        }

        foreach (var topic in _responses.Keys)
        {
            if (lower.Contains(topic, StringComparison.OrdinalIgnoreCase) ||
                (topic == "browsing" && lower.Contains("browser", StringComparison.OrdinalIgnoreCase)))
            {

                intent = topic;
                _memory.CurrentTopic = topic;

                return ApplySentiment(
                    _responses[topic][_random.Next(_responses[topic].Count)],
                    sentiment);


            }
        }
        if (ContainsAny(lower, "what is my name", "what's my name", "do you remember my name"))
        {
            intent = "memory";
            return $"Your name is {UserName}. I remember it from our conversation.";
        }
        if (ContainsAny(lower, "hello", "hi", "hey"))
        {
            intent = "general";
            return $"Hello, {UserName}! Ask me about a cybersecurity topic or choose a quick action.";
        }

        intent = "unknown";
        return "I can help with passwords, phishing, scams, privacy, safe browsing, tasks, reminders, the quiz, and activity history. Try asking about one of those topics.";
    }

    public string GetTaskDescription(string title)
    {
        string lower = title.ToLowerInvariant();
        if (lower.Contains("2fa") || lower.Contains("two-factor"))
            return "Enable two-factor authentication on an important account.";
        if (lower.Contains("privacy"))
            return "Review account privacy settings and reduce unnecessary information sharing.";
        if (lower.Contains("password"))
            return "Review password security and use a strong, unique password where needed.";
        return $"Complete the cybersecurity task: {title}.";
    }


    private string FollowUpResponse()
    {
        string topic = _memory.CurrentTopic;

        // Use the favourite topic if no topic has been discussed yet.
        if (string.IsNullOrWhiteSpace(topic))
        {
            topic = FavouriteTopic.ToLowerInvariant() switch
            {
                "phishing" => "phishing",
                "privacy" => "privacy",
                "safe browsing" => "browsing",
                "password safety" => "password",
                "scams" => "scam",
                _ => "password"
            };
        }

        // Make sure the topic exists in the response dictionary.
        if (!_responses.ContainsKey(topic))
        {
            topic = "password";
        }

        string response =
            _responses[topic][_random.Next(_responses[topic].Count)];

        return $"Here is another tip about {topic}: {response}";
    }


    private static string ApplySentiment(string response, string sentiment)
    {
        return sentiment switch
        {
            "worried" => "It is understandable to feel concerned. " + response + " Taking one small step at a time can make cybersecurity easier to manage.",
            "curious" => "Great question! " + response,
            "frustrated" => "Let's keep it simple. " + response,
            _ => response
        };
    }

    private static bool ContainsAny(string input, params string[] values) =>
        values.Any(value => input.Contains(value, StringComparison.OrdinalIgnoreCase));
}
