using Microsoft.Extensions.Logging;

public class FileLoggerProvider(string filePath) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new FileLogger(filePath);
    public void Dispose() { }
}

public class FileLogger(string filePath) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        File.AppendAllText(filePath,
            $"[{DateTime.Now:HH:mm:ss}] [{logLevel}] {formatter(state, exception)}{Environment.NewLine}");
    }
}