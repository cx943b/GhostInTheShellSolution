using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace GhostInTheShell.Tests
{
    // https://stackoverflow.com/questions/52707702/how-do-you-mock-ilogger-loginformation
    public static class LoggerMockFactory
    {
        public static ILogger<T> CreateLogger<T>()
        {
            Mock<ILogger<T>> mkLogger = new Mock<ILogger<T>>();
            mkLogger
                .Setup(logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0];
                    var eventId = (EventId)invocation.Arguments[1];
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = (string)invokeMethod?.Invoke(formatter, new[] { state, exception });

                    if(logLevel is not LogLevel.Information)
                        Trace.WriteLine($"{logLevel}: {logMessage}");
                }));

            return mkLogger.Object;
        }
    }
}