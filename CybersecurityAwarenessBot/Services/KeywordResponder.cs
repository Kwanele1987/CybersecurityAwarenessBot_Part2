
using System;
using System.Collections.Generic;

namespace CybersecurityAwarenessBot.Services
{
    public class KeywordResponder
    {
        private readonly Dictionary<string, List<string>> responses =
            new Dictionary<string, List<string>>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
                {
                    "Use a strong, unique password for each account.",
                    "Avoid using personal information in passwords.",
                    "Consider using a trusted password manager."
                },

                ["phishing"] = new List<string>
                {
                    "Check the sender before trusting an email.",
                    "Avoid clicking unexpected links.",
                    "Never share passwords through suspicious messages."
                },

                ["scam"] = new List<string>
                {
                    "Be cautious of messages that pressure you to act urgently.",
                    "Verify payment requests through a trusted contact.",
                    "Check whether a message is legitimate before sharing information."
                },

                ["privacy"] = new List<string>
                {
                    "Review your privacy settings regularly.",
                    "Limit unnecessary app permissions.",
                    "Think carefully before sharing personal information online."
                },

                ["malware"] = new List<string>
                {
                    "Keep your operating system updated.",
                    "Download software from trusted sources.",
                    "Avoid unexpected attachments from unknown senders."
                }
            };

        private readonly Random random = new Random();

        public string DetectTopic(string input)
        {
            string text = input.ToLowerInvariant();

            if (text.Contains("phishing") || text.Contains("phish"))
                return "phishing";

            if (text.Contains("password") || text.Contains("passcode"))
                return "password";

            if (text.Contains("scam") || text.Contains("fraud"))
                return "scam";

            if (text.Contains("privacy") ||
                text.Contains("personal information"))
                return "privacy";

            if (text.Contains("malware") || text.Contains("virus"))
                return "malware";

            return "";
        }

        public string GetRandomResponse(string topic)
        {
            if (responses.TryGetValue(topic, out List<string> answers)
                && answers.Count > 0)
            {
                return answers[random.Next(answers.Count)];
            }

            return "Please ask me about passwords, phishing, scams, privacy or malware.";
        }
    }
}
