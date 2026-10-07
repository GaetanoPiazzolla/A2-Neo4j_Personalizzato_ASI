using System;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Neo4jBackend.Extensions;

public class Neo4jLoggerAdapter(ILoggerFactory loggerFactory) : INeo4jLogger
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("Neo4j.Driver");

    public void Error(Exception cause, string message, params object[] args)
        => _logger.LogError(cause, message, args);

    public void Warn(Exception cause, string message, params object[] args)
        => _logger.LogWarning(cause, message, args);

    public void Info(string message, params object[] args)
        => _logger.LogInformation(message, args);

    public void Debug(string message, params object[] args)
        => _logger.LogDebug(message, args);

    public void Trace(string message, params object[] args)
        => _logger.LogTrace(message, args);

    public bool IsTraceEnabled() => _logger.IsEnabled(LogLevel.Trace);

    public bool IsDebugEnabled() => _logger.IsEnabled(LogLevel.Debug);
}
