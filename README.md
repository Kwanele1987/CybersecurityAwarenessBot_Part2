# Cybersecurity Awareness Assistant

## PROG6221/w – Programming 2A

**Student:** Kwanele Langa  
**Student Number:** ST10500677

## Overview

The Cybersecurity Awareness Assistant is a C# application designed to help users learn about common cybersecurity topics through an interactive chatbot and practical features.

The project includes a command-line chatbot for Part 1 and a WPF graphical application for the final project. The WPF application brings together the chatbot, cybersecurity quiz, task assistant, MySQL storage and activity logging.

## Main Features

### Part 1 – Console Chatbot

- Recorded voice greeting using a WAV file
- Cybersecurity-themed ASCII logo
- Personalised greeting using the user's name
- Keyword-based cybersecurity responses
- Input validation and fallback responses
- Separate classes for the chatbot, user profile, response handling, voice greeting and ASCII art

### WPF Application

- Graphical chatbot interface
- Cybersecurity keyword recognition
- Varied/random chatbot responses
- Conversation follow-up handling
- Memory for the user's name and favourite cybersecurity topic
- Simple sentiment detection
- Support for cybersecurity-related tasks
- Cybersecurity quiz with 12 questions
- Activity log for recent application actions

### Task Assistant and MySQL

The Task Assistant allows users to:

- Add cybersecurity-related tasks
- Enter a task title and description
- Set an optional reminder date
- Mark tasks as completed
- Delete tasks

Task information is stored in a MySQL database using the `MySqlTaskRepository` service.

## Project Structure

```text
CybersecurityAwarenessBot
│
├── CybersecurityAwarenessBot.sln
├── CybersecurityAwarenessBot
│   ├── App.xaml
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Models
│   ├── Services
│   ├── Audio
│   ├── database.sql
│   └── CybersecurityAwarenessBot.csproj
│
└── CybersecurityAwarenessBot.Part1
    ├── Program.cs
    ├── Chatbot.cs
    ├── ResponseHandler.cs
    ├── UserProfile.cs
    ├── AsciiArt.cs
    ├── VoiceGreeting.cs
    ├── Audio
    └── CybersecurityAwarenessBot.Part1.csproj
```

## Requirements

- Windows
- Visual Studio with .NET 8 support
- MySQL Server
- MySQL Workbench (recommended for database setup)
- Internet connection for restoring NuGet packages

## MySQL Setup

1. Start the MySQL Server.
2. Open MySQL Workbench.
3. Open and run `database.sql` to create the `cybersecurity_bot` database and `tasks` table.
4. Open `CybersecurityAwarenessBot/Services/MySqlTaskRepository.cs`.
5. Update the MySQL connection settings so that the `Server`, `User` and `Password` values match your local MySQL installation.
6. Build and run the WPF application.

The application also attempts to create the database and `tasks` table automatically when it starts, provided the MySQL account has the required permissions.

> **Security:** Do not commit a real database password to GitHub. Keep the password as a local value on your computer.

## Voice Greeting

The Part 1 and WPF applications support a recorded voice greeting.

To use the feature, place the WAV file at:

```text
CybersecurityAwarenessBot.Part1/Audio/greeting.wav
```

and for the WPF application:

```text
CybersecurityAwarenessBot/Audio/greeting.wav
```

The WAV file is copied to the application's output directory automatically when the project is built.

## Running the Solution

1. Open `CybersecurityAwarenessBot.sln` in Visual Studio.
2. Restore the NuGet packages if prompted.
3. For the final WPF application, set `CybersecurityAwarenessBot` as the startup project.
4. Build the solution.
5. Press **F5** to run the application.

### Running Part 1

For the console chatbot demonstration:

1. Set `CybersecurityAwarenessBot.Part1` as the startup project.
2. Press **F5**.
3. Enter a name when prompted.
4. Try questions about phishing, passwords, safe browsing or the chatbot's purpose.

## Example Chatbot Interactions

The chatbot can respond to topics and commands such as:

- `What is phishing?`
- `Tell me about password safety`
- `How can I browse safely?`
- `What is your purpose?`
- `I'm worried about online security`
- `My favourite topic is phishing`
- `What is my favourite topic?`
- `Give me another tip`
- `Show my tasks`
- `Start quiz`
- `Show activity log`

## Cybersecurity Quiz

The application includes a 12-question cybersecurity quiz. Each question provides feedback after an answer is selected, and the final result is displayed as both a score and percentage.

## Activity Log

The Activity Log records recent actions and interactions, including relevant chatbot activity and task actions. This provides a simple record of activity while the application is being used.

## Version Control and CI

The project is maintained using GitHub for version control. Meaningful commits were used to record development changes, and GitHub Actions is configured to build the solution automatically when repository changes are made.

The project also contains version releases:

- **v1.0** – Initial Project
- **v1.1** – Chatbot Memory Enhancement
- **v1.2** – Final Application Improvements

## Presentation

The project includes a video demonstration showing the application running and explaining its main functionality and logic. The presentation uses the student's own recorded voice.
Watch the demonstration of the Cybersecurity Awareness Assistant:

Click here to watch the project presentation on YouTube

The presentation demonstrates the application and its features.
