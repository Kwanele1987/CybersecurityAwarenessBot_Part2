using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using CybersecurityAwarenessBot.Models;
using CybersecurityAwarenessBot.Services;

namespace CybersecurityAwarenessBot;

public partial class MainWindow : Window
{
    private readonly ChatbotService _chatbot;
    private readonly MySqlTaskRepository _repository;
    private readonly ActivityLogService _activityLog;
    private readonly VoiceGreeting _voiceGreeting;
    private readonly ObservableCollection<CyberTask> _tasks = new();

    private readonly List<QuizQuestion> _quizQuestions = CybersecurityAwarenessBot.Models.QuizQuestion.GetQuestions();
    private int _quizIndex = -1;
    private int _quizScore;
    private bool _quizAnswerLocked;
    private QuizQuestion? _currentQuestion;

    public MainWindow()
    {
        InitializeComponent();

        _chatbot = new ChatbotService();
        _repository = new MySqlTaskRepository();
        _activityLog = new ActivityLogService();
        _voiceGreeting = new VoiceGreeting();

        AsciiArtText.Text = AsciiArtService.GetLogo();
        NameBox.Text = _chatbot.UserName;
        TopicBox.SelectedIndex = 0;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AddBotMessage($"Hello! Welcome to the Cybersecurity Awareness Assistant, {NameBox.Text}. I can help you learn about passwords, phishing, privacy, safe browsing, tasks and reminders, quizzes, and more.");
        PlayGreeting();
        await LoadTasksAsync();
        RefreshActivityLog();
        UpdateMemorySummary();
    }

    private void PlayGreeting_Click(object sender, RoutedEventArgs e) => PlayGreeting();

    private void PlayGreeting()
    {
        bool played = _voiceGreeting.PlayGreeting(out string message);
        StatusText.Text = message;

        if (played)
        {
            _activityLog.Add("Voice greeting played.");
            RefreshActivityLog();
        }
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        await ProcessChatInputAsync();
    }

    private async void ChatInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await ProcessChatInputAsync();
        }
    }

    private async Task ProcessChatInputAsync()
    {
        string input = ChatInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            AddBotMessage("Please enter a question or choose one of the quick actions. I am ready to help.");
            return;
        }

        AddUserMessage(input);
        ChatInput.Clear();

        if (TryHandleMemory(input))
            return;

        string sentiment = _chatbot.DetectSentiment(input);
        string response = _chatbot.GetResponse(input, sentiment, out string intent);

        if (intent == "quiz")
        {
            MainTabs.SelectedIndex = 2;
            StartQuiz();
        }
        else if (intent == "tasks")
        {
            MainTabs.SelectedIndex = 1;
        }
        else if (intent == "log")
        {
            MainTabs.SelectedIndex = 3;
            RefreshActivityLog();
        }
        else if (intent == "add_task")
        {
            MainTabs.SelectedIndex = 1;
            string taskText = ExtractTaskText(input);
            if (!string.IsNullOrWhiteSpace(taskText))
            {
                TaskTitleBox.Text = taskText;
                TaskDescriptionBox.Text = _chatbot.GetTaskDescription(taskText);
                response += "\nI have prepared the task fields for you. Add an optional reminder date and select Add Task.";
            }
        }

        AddBotMessage(response);
        _activityLog.Add($"NLP interaction: {intent} detected.");
        RefreshActivityLog();

        await Task.CompletedTask;
    }

    private bool TryHandleMemory(string input)
    {
        Match nameMatch = Regex.Match(input, @"\b(?:my name is|call me)\s+([A-Za-z][A-Za-z '-]{1,40})\b",
            RegexOptions.IgnoreCase);
        if (nameMatch.Success)
        {
            string name = nameMatch.Groups[1].Value.Trim();
            _chatbot.UserName = name;
            NameBox.Text = name;
            AddBotMessage($"Nice to meet you, {name}. I will remember your name during this conversation.");
            _activityLog.Add($"Remembered user's name as {name}.");
            RefreshActivityLog();
            UpdateMemorySummary();
            return true;
        }

        Match topicMatch = Regex.Match(input,
            @"(?:interested in|favourite topic is|favorite topic is)\s+(password|phishing|privacy|safe browsing)",
            RegexOptions.IgnoreCase);
        if (topicMatch.Success)
        {
            string topic = topicMatch.Groups[1].Value;
            _chatbot.FavouriteTopic = topic;
            SelectTopic(topic);
            AddBotMessage($"Great, {NameBox.Text}! I will remember that your favourite cybersecurity topic is {topic}.");
            _activityLog.Add($"Remembered favourite topic: {topic}.");
            RefreshActivityLog();
            UpdateMemorySummary();
            return true;
        }

        if (input.Contains("what do you remember", StringComparison.OrdinalIgnoreCase))
        {
            AddBotMessage($"I remember your name as {NameBox.Text} and your favourite topic as {_chatbot.FavouriteTopic}. I use this information only to personalise this current application session.");
            return true;
        }

        return false;
    }

    private string ExtractTaskText(string input)
    {
        string cleaned = Regex.Replace(input,
            @"^(please\s+)?(add|create|set up|make)\s+(a\s+)?(task|reminder)\s*(to|for)?\s*",
            "", RegexOptions.IgnoreCase).Trim();

        if (string.IsNullOrWhiteSpace(cleaned))
            cleaned = "Review cybersecurity settings";

        cleaned = Regex.Replace(cleaned, @"\b(?:tomorrow|today|in\s+\d+\s+days?)\b.*$", "",
            RegexOptions.IgnoreCase).Trim();

        return char.ToUpper(cleaned[0]) + cleaned[1..];
    }

    private void AddBotMessage(string message)
    {
        ChatHistory.Document.Blocks.Add(CreateParagraph("Bot", message, "#17324D"));
        ChatHistory.ScrollToEnd();
    }

    private void AddUserMessage(string message)
    {
        ChatHistory.Document.Blocks.Add(CreateParagraph(NameBox.Text, message, "#2F80ED"));
        ChatHistory.ScrollToEnd();
    }

    private Paragraph CreateParagraph(string speaker, string message, string colour)
    {
        var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 12) };
        var speakerRun = new Run($"{speaker}: ") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colour)) };
        paragraph.Inlines.Add(speakerRun);
        paragraph.Inlines.Add(new Run(message));
        return paragraph;
    }

    private void QuickPassword_Click(object sender, RoutedEventArgs e) => SendQuick("Tell me about password safety.");
    private void QuickPhishing_Click(object sender, RoutedEventArgs e) => SendQuick("Give me a phishing tip.");
    private void QuickBrowsing_Click(object sender, RoutedEventArgs e) => SendQuick("How can I browse safely?");
    private void QuickPrivacy_Click(object sender, RoutedEventArgs e) => SendQuick("Tell me about privacy.");
    private void SendQuick(string text)
    {
        AddUserMessage(text);
        AddBotMessage(_chatbot.GetResponse(text, _chatbot.DetectSentiment(text), out _));
    }

    private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_chatbot != null && !string.IsNullOrWhiteSpace(NameBox.Text))
        {
            _chatbot.UserName = NameBox.Text.Trim();
            UpdateMemorySummary();
        }
    }

    private void TopicBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_chatbot == null || TopicBox.SelectedItem is not ComboBoxItem item)
            return;

        _chatbot.FavouriteTopic = item.Content?.ToString() ?? "Password Safety";
        UpdateMemorySummary();
    }

    private void SelectTopic(string topic)
    {
        for (int i = 0; i < TopicBox.Items.Count; i++)
        {
            if (TopicBox.Items[i] is ComboBoxItem item &&
                item.Content?.ToString()?.Contains(topic, StringComparison.OrdinalIgnoreCase) == true)
            {
                TopicBox.SelectedIndex = i;
                return;
            }
        }
    }

    private void UpdateMemorySummary()
    {
        if (MemorySummary == null || _chatbot == null) return;
        MemorySummary.Text = $"Name: {_chatbot.UserName}\nFavourite topic: {_chatbot.FavouriteTopic}";
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            await _repository.EnsureDatabaseAndTableAsync();
            var tasks = await _repository.GetTasksAsync();

            _tasks.Clear();
            foreach (var task in tasks)
                _tasks.Add(task);

            TasksGrid.ItemsSource = _tasks;
            StatusText.Text = $"Loaded {_tasks.Count} task(s) from MySQL.";
        }
        catch (Exception ex)
        {
            StatusText.Text = "MySQL connection is not available. Check app settings and make sure MySQL is running.";
            AddBotMessage($"I could not connect to the MySQL database yet. The rest of the chatbot remains available. Database message: {ex.Message}");
        }
    }

    private async void AddTask_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TaskTitleBox.Text))
        {
            MessageBox.Show("Please enter a task title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(TaskDescriptionBox.Text))
        {
            MessageBox.Show("Please enter a task description.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var task = new CyberTask
        {
            Title = TaskTitleBox.Text.Trim(),
            Description = string.IsNullOrWhiteSpace(TaskDescriptionBox.Text)
                ? _chatbot.GetTaskDescription(TaskTitleBox.Text)
                : TaskDescriptionBox.Text.Trim(),
            ReminderDate = ReminderDatePicker.SelectedDate
        };

        try
        {
            await _repository.AddTaskAsync(task);
            _activityLog.Add($"Task added: {task.Title}" + (task.ReminderDate.HasValue ? $" (Reminder: {task.ReminderDate:yyyy-MM-dd})" : " (No reminder)"));
            TaskTitleBox.Clear();
            TaskDescriptionBox.Clear();
            ReminderDatePicker.SelectedDate = null;
            await LoadTasksAsync();
            RefreshActivityLog();
            AddBotMessage($"Task added: \"{task.Title}\". " +
                          (task.ReminderDate.HasValue ? $"Reminder set for {task.ReminderDate:dd MMM yyyy}." : "No reminder was set."));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save the task.\n\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CompleteTask_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is not CyberTask task) return;

        try
        {
            await _repository.MarkCompletedAsync(task.Id);
            _activityLog.Add($"Task completed: {task.Title}");
            await LoadTasksAsync();
            RefreshActivityLog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is not CyberTask task) return;

        if (MessageBox.Show($"Delete '{task.Title}'?", "Confirm Delete",
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            await _repository.DeleteTaskAsync(task.Id);
            _activityLog.Add($"Task deleted: {task.Title}");
            await LoadTasksAsync();
            RefreshActivityLog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenTasks_Click(object sender, RoutedEventArgs e) => MainTabs.SelectedIndex = 1;

    private void StartQuiz_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 2;
        StartQuiz();
    }

    private void StartQuiz()
    {
        _quizIndex = 0;
        _quizScore = 0;
        _quizAnswerLocked = false;
        _currentQuestion = _quizQuestions[_quizIndex];
        _activityLog.Add("Quiz started.");
        RefreshActivityLog();
        DisplayQuizQuestion();
    }

    private void DisplayQuizQuestion()
    {
        if (_currentQuestion == null) return;

        QuizProgress.Text = $"Question {_quizIndex + 1} of {_quizQuestions.Count}";
        QuizQuestion.Text = _currentQuestion.Question;
        QuizFeedback.Text = "";
        QuizFinalScore.Text = "";
        QuizOptions.Children.Clear();

        for (int i = 0; i < _currentQuestion.Options.Count; i++)
        {
            var button = new Button
            {
                Content = $"{(char)('A' + i)}) {_currentQuestion.Options[i]}",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Tag = i,
                Height = 45
            };
            button.Click += QuizOption_Click;
            QuizOptions.Children.Add(button);
        }

        NextQuizButton.Content = "Answer a question";
        NextQuizButton.IsEnabled = false;
    }

    private void QuizOption_Click(object sender, RoutedEventArgs e)
    {
        if (_quizAnswerLocked || _currentQuestion == null) return;

        _quizAnswerLocked = true;
        int selected = (int)((Button)sender).Tag;

        bool correct = selected == _currentQuestion.CorrectIndex;
        if (correct) _quizScore++;

        QuizFeedback.Text = correct
            ? $"Correct! {_currentQuestion.Explanation}"
            : $"Not quite. The correct answer is {_currentQuestion.Options[_currentQuestion.CorrectIndex]}. {_currentQuestion.Explanation}";

        foreach (Button child in QuizOptions.Children)
            child.IsEnabled = false;

        NextQuizButton.IsEnabled = true;
        NextQuizButton.Content = _quizIndex == _quizQuestions.Count - 1 ? "Finish Quiz" : "Next Question";
    }

    private void NextQuizButton_Click(object sender, RoutedEventArgs e)
    {
        if (_quizIndex == -1)
        {
            StartQuiz();
            return;
        }

        if (!_quizAnswerLocked) return;

        if (_quizIndex < _quizQuestions.Count - 1)
        {
            _quizIndex++;
            _currentQuestion = _quizQuestions[_quizIndex];
            _quizAnswerLocked = false;
            DisplayQuizQuestion();
            return;
        }

        QuizQuestion.Text = "Quiz complete!";
        QuizOptions.Children.Clear();
        QuizProgress.Text = "Finished";
        double quizPercentage = (_quizScore * 100.0) / _quizQuestions.Count;

        QuizFinalScore.Text = $"Final score: {_quizScore}/{_quizQuestions.Count} ({quizPercentage:0}%). " +
            (quizPercentage >= 80
                ? "Great job! You have a strong cybersecurity awareness foundation."
                : "Keep learning to strengthen your cybersecurity awareness.");
        QuizFeedback.Text = "Review the chatbot topics and try the quiz again to improve your score.";
        NextQuizButton.Content = "Restart Quiz";
        _quizIndex = -1;
        _currentQuestion = null;
        _activityLog.Add($"Quiz completed with score {_quizScore}/{_quizQuestions.Count}.");
        RefreshActivityLog();
    }

    private void ShowLog_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 3;
        RefreshActivityLog();
    }


    private void RefreshActivityLog()
    {
        if (_activityLog == null || ActivityList == null)
            return;

        ActivityList.ItemsSource = _activityLog.GetRecent(10);
    }
    private void RefreshLog_Click(object sender, RoutedEventArgs e)
    {
        RefreshActivityLog();
    }
}


