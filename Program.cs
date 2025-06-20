using System;
using System.IO;
using System.Linq;

try
{
    var arguments = ParseArguments(args);

    if (arguments == null)
    {
        ShowHelp();
        return;
    }

    if (!File.Exists(arguments.FilePath))
    {
        Console.WriteLine($"Файл не найден: {arguments.FilePath}.");
        return;
    }

    var comparison = arguments.UnregisteredSearch
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    var linesWithKeyword = File.ReadLines(arguments.FilePath)
    .Where(line => line.IndexOf(arguments.KeyWord, comparison) >= 0).ToList();

    File.WriteAllLines(arguments.OutputPath, linesWithKeyword);

    Console.WriteLine($"найдено {linesWithKeyword.Count} строк с ключевым словом '{arguments.KeyWord}'." +
        $" Результаты сохранены в файл {arguments.OutputPath}.");
}
catch (Exception ex)
{
    var errorMessage = $"Ошибка при обработке файла: {ex.Message}";
    Console.WriteLine(errorMessage);
    Logger.LogError(errorMessage, ex);
    throw;
}

static void ShowHelp()
{
    Console.WriteLine("Использование: LogParser.exe --file \"путь_к_файлу\" --keyword \"ключевое_слово\" --output \"выходной_файл\" [--unregistered | -u]");
    Console.WriteLine("Пример1: LogParser.exe --file \"C:\\logs\\app.log\" --keyword \"Timeout\" --output \"errors.txt\"");
    Console.WriteLine("Пример2: LogParser.exe --file \"C:\\logs\\app.log\" --keyword \"Timeout\" --output \"errors.txt\" -u");
}

static Arguments ParseArguments(string[] args)
{
    var arguments = new Arguments();

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--file":
                if (i + 1 < args.Length)
                    arguments.FilePath = args[++i];
                break;
            case "--keyword":
                if (i + 1 < args.Length)
                    arguments.KeyWord = args[++i];
                break;
            case "--output":
                if (i + 1 < args.Length)
                    arguments.OutputPath = args[++i];
                break;
            case "--unregistered":
            case "-u":
                arguments.UnregisteredSearch = true;
                break;
        }
    }

    if (string.IsNullOrEmpty(arguments.FilePath) ||
        string.IsNullOrEmpty(arguments.KeyWord) ||
        string.IsNullOrEmpty(arguments.OutputPath))
    {
        return null;
    }

    return arguments;
}

class Arguments
{
    public string FilePath { get; set; } = "";
    public string KeyWord { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public bool UnregisteredSearch { get; set; } = false;
}

public static class Logger
{
    private static readonly string LogFilePath = "error_log.txt";
    private static readonly object LockObject = new object();

    public static void LogError(string message, Exception ex = null)
    {
        lock (LockObject)
        {
            try
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
                if (ex != null)
                {
                    logMessage += $"\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}";
                }
                logMessage += "\n--------------------------------------------------\n";

                File.AppendAllText(LogFilePath, logMessage);
            }
            catch (Exception loggingEx)
            {
                Console.WriteLine($"Не удалось записать в лог: {loggingEx.Message}");
            }
        }
    }
}